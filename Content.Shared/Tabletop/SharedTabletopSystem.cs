using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Tabletop.Components;
using Content.Shared.Tabletop.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Tabletop
{
    public abstract partial class SharedTabletopSystem : EntitySystem
    {
        [Dependency] protected readonly ActionBlockerSystem ActionBlockerSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedTransformSystem _transforms = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<TabletopDraggableComponent, ComponentGetState>(GetDraggableState);
            SubscribeAllEvent<TabletopDraggingPlayerChangedEvent>(OnDraggingPlayerChanged);
            SubscribeAllEvent<TabletopMoveEvent>(OnTabletopMove);

            InitializeShogi();
        }

        /// <summary>
        ///     Move an entity which is dragged by the user, but check if they are allowed to do so and to these coordinates
        /// </summary>
        protected virtual void OnTabletopMove(TabletopMoveEvent msg, EntitySessionEventArgs args)
        {
            if (args.SenderSession is not { AttachedEntity: { } playerEntity } playerSession)
                return;

            if (!CanSeeTable(playerEntity, msg.TableUid) || !CanDrag(playerEntity, msg.MovedEntityUid, out _))
                return;

            // Move the entity and dirty it (we use the map ID from the entity so noone can try to be funny and move the item to another map)
            var transform = EntityManager.GetComponent<TransformComponent>(msg.MovedEntityUid);
            _transforms.SetParent(msg.MovedEntityUid, transform, _mapMan.GetMapEntityId(transform.MapID));
            _transforms.SetLocalPositionNoLerp(transform, msg.Coordinates.Position);
        }

        private void GetDraggableState(EntityUid uid, TabletopDraggableComponent component, ref ComponentGetState args)
        {
            args.State = new TabletopDraggableComponentState(component.DraggingPlayer);
        }

        private void OnDraggingPlayerChanged(TabletopDraggingPlayerChangedEvent msg, EntitySessionEventArgs args)
        {
            var dragged = msg.DraggedEntityUid;

            if (!TryComp(dragged, out TabletopDraggableComponent? draggableComponent))
                return;

            draggableComponent.DraggingPlayer = msg.IsDragging ? args.SenderSession.UserId : null;
            Dirty(draggableComponent);

            if (!TryComp(dragged, out AppearanceComponent? appearance))
                return;

            if (draggableComponent.DraggingPlayer != null)
            {
                _appearance.SetData(dragged, TabletopItemVisuals.Scale, new Vector2(1.25f, 1.25f), appearance);
                _appearance.SetData(dragged, TabletopItemVisuals.DrawDepth, (int) DrawDepth.DrawDepth.Items + 1, appearance);
            }
            else
            {
                _appearance.SetData(dragged, TabletopItemVisuals.Scale, Vector2.One, appearance);
                _appearance.SetData(dragged, TabletopItemVisuals.DrawDepth, (int) DrawDepth.DrawDepth.Items, appearance);
            }
        }


        [Serializable, NetSerializable]
        public sealed class TabletopDraggableComponentState : ComponentState
        {
            public NetUserId? DraggingPlayer;

            public TabletopDraggableComponentState(NetUserId? draggingPlayer)
            {
                DraggingPlayer = draggingPlayer;
            }
        }

        #region Utility

        /// <summary>
        /// Whether the table exists, and the player can interact with it.
        /// </summary>
        /// <param name="playerEntity">The player entity to check.</param>
        /// <param name="table">The table entity to check.</param>
        protected bool CanSeeTable(EntityUid playerEntity, EntityUid? table)
        {
            // Table may have been deleted, hence TryComp
            if (!TryComp(table, out MetaDataComponent? meta)
                || meta.EntityLifeStage >= EntityLifeStage.Terminating
                || (meta.Flags & MetaDataFlags.InContainer) == MetaDataFlags.InContainer)
            {
                return false;
            }

            return _interactionSystem.InRangeUnobstructed(playerEntity, table.Value) && ActionBlockerSystem.CanInteract(playerEntity, table);
        }

        protected bool CanDrag(EntityUid playerEntity, EntityUid target, [NotNullWhen(true)] out TabletopDraggableComponent? draggable)
        {
            if (!TryComp(target, out draggable))
                return false;

            // CanSeeTable checks interaction action blockers. So no need to check them here.
            // If this ever changes, so that ghosts can spectate games, then the check needs to be moved here.

            return TryComp(playerEntity, out SharedHandsComponent? hands) && hands.Hands.Count > 0;
        }
        #endregion
    }
}
