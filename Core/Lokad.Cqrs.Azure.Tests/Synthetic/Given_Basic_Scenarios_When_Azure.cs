﻿#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Basic_Scenarios_When_Azure : Given_Basic_Scenarios
    {
        protected override Given_Basic_Scenarios.Setup ConfigureComponents(Envelope.IEnvelopeStreamer config)
        {
            // Azure dev is implemented via WS on top of SQL on top of FS.
            // this can be slow. And it will be
            TestSpeed = 7000;

            var dev = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);
            return new Setup
                {
                    Store = dev.CreateNuclear(),
                    Inbox = dev.CreateInbox("test-incoming", visibilityTimeout : TimeSpan.FromSeconds(1)),
                    Sender = dev.CreateSimpleSender(config, "test-incoming")
                };
        }
    }
}