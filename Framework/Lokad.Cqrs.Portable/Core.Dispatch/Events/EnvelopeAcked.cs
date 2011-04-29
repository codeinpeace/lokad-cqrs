﻿namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class EnvelopeAcked : ISystemEvent
    {
        public string QueueName { get; private set; }
        public string EnvelopeId { get; private set; }

        public EnvelopeAcked(string queueName, string envelopeId)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
        }

        public override string ToString()
        {
            return string.Format("[{0}] acked at '{1}'", EnvelopeId, QueueName);
        }
    }
}