﻿using Robust.Shared.Serialization;

namespace Content.Shared.MachineLinking
{
    [Serializable, NetSerializable]
    public sealed class SignalPortsState : BoundUserInterfaceState
    {
        public readonly string TransmitterName;
        /// <summary>
        /// A List of all ports on the selected transmitter
        /// </summary>
        public readonly List<string> TransmitterPorts;

        public readonly string ReceiverName;
        /// <summary>
        /// A List of all ports on the selected receiver
        /// </summary>
        public readonly List<string> ReceiverPorts;

        public readonly List<(int, int)> Links;

        public SignalPortsState(string transmitterName, List<string> transmitterPorts, string receiverName, List<string> receiverPorts, List<(int, int)> links)
        {
            TransmitterName = transmitterName;
            TransmitterPorts = transmitterPorts;
            ReceiverName = receiverName;
            ReceiverPorts = receiverPorts;
            Links = links;
        }
    }

    [Serializable, NetSerializable]
    public sealed class SignalPortSelected : BoundUserInterfaceMessage
    {
        public readonly string TransmitterPort;
        public readonly string ReceiverPort;

        public SignalPortSelected(string transmitterPort, string receiverPort)
        {
            TransmitterPort = transmitterPort;
            ReceiverPort = receiverPort;
        }
    }

    [Serializable, NetSerializable]
    public sealed class LinkerClearSelected : BoundUserInterfaceMessage { }

    [Serializable, NetSerializable]
    public sealed class LinkerLinkDefaultSelected : BoundUserInterfaceMessage { }


    [Serializable, NetSerializable]
    public sealed class SignalTimerState : BoundUserInterfaceState
    {
        public bool State { get; }
        public float Length { get; }
        public float Remaining { get; }

        public SignalTimerState(bool state, float length, float remaining)
        {
            State = state;
            Length = length;
            Remaining = remaining;
        }
    }

    [Serializable, NetSerializable]
    public sealed class SignalTimerLengthChangedMessage : BoundUserInterfaceMessage
    {
        public float Length { get; }

        public SignalTimerLengthChangedMessage(float length)
        {
            Length = length;
        }
    }

    [Serializable, NetSerializable]
    public sealed class SignalTimerStartedMessage : BoundUserInterfaceMessage
    {
    }
}
