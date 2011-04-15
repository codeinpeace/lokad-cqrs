﻿using System;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzureSender
{
	public sealed class StatelessAzureQueueWriter : IQueueWriter
	{
		//public void SendAsSingleMessage(object[] items)
		//{
		//    if (items.Length == 0)
		//        return;

		//    var builder = BuildEnvelopeFromItems(items);
		//    var packed = PrepareCloudMessage(builder);
		//    _queue.AddMessage(packed);
		//}

		//public void ForwardMessage(MessageEnvelope envelope)
		//{
		//    var builder = CopyEnvelope(envelope);
		//    var packed = PrepareCloudMessage(builder);
		//    _queue.AddMessage(packed);
		//}

		public void PutMessage(MessageEnvelope envelope)
		{
			var packed = PrepareCloudMessage(envelope);
			_queue.AddMessage(packed);
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PrepareCloudMessage(MessageEnvelope builder)
		{
			var buffer = MessageUtil.SaveDataMessage(builder, _serializer);
			if (buffer.Length < CloudQueueLimit)
			{
				// write message to queue
				return new CloudQueueMessage(buffer);
			}
			// ok, we didn't fit, so create reference message
			var referenceId = DateTimeOffset.UtcNow.ToString(DateFormatInBlobName) + "-" + builder.EnvelopeId;
			_cloudBlob.GetBlobReference(referenceId).UploadByteArray(buffer);
			var reference = new MessageReference(builder.EnvelopeId, _cloudBlob.Uri.ToString(), referenceId);
			var blob = MessageUtil.SaveReferenceMessage(reference);
			return new CloudQueueMessage(blob);
		}

		//static MessageEnvelopeBuilder CopyEnvelope(MessageEnvelope envelope)
		//{
		//    var builder = new MessageEnvelopeBuilder(envelope.EnvelopeId);

		//    foreach (var item in envelope.Items)
		//    {
		//        var save = new MessageItemToSave(item.MappedType, item.Content);
		//        foreach (var attribute in item.GetAllAttributes())
		//        {
		//            save.Attributes.Add(attribute);
		//        }
		//        builder.Items.Add(save);
		//    }
		//    foreach (var attribute in envelope.GetAllAttributes())
		//    {
		//        builder.Attributes.Add(attribute);
		//    }
		//    return builder;
		//}

		public StatelessAzureQueueWriter(IMessageSerializer serializer, CloudStorageAccount account, string queueName)
		{
			_serializer = serializer;
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);
		}

		public void Init()
		{
			_queue.CreateIfNotExist();
			_cloudBlob.CreateIfNotExist();
		}


		const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
		readonly IMessageSerializer _serializer;
		readonly CloudBlobContainer _cloudBlob;
		readonly CloudQueue _queue;
	}
}