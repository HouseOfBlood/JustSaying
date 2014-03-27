using System.Collections.Generic;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using JustEat.Simples.NotificationStack.AwsTools;
using JustEat.Simples.NotificationStack.Messaging.Monitoring;
using JustEat.Testing;
using NSubstitute;
using NUnit.Framework;
using SimpleMessageMule.TestingFramework;

namespace AwsTools.UnitTests.SqsNotificationListener
{
    public class WhenThereAreNoMessagesToProcess : BehaviourTest<JustEat.Simples.NotificationStack.AwsTools.SqsNotificationListener>
    {
        private readonly IAmazonSQS _sqs = Substitute.For<IAmazonSQS>();
        private int _callCount;

        protected override JustEat.Simples.NotificationStack.AwsTools.SqsNotificationListener CreateSystemUnderTest()
        {
            return new JustEat.Simples.NotificationStack.AwsTools.SqsNotificationListener(new SqsQueueByUrl("", _sqs), null, new NullMessageFootprintStore(), Substitute.For<IMessageMonitor>());
        }

        protected override void Given()
        {
            _sqs.ReceiveMessage(Arg.Any<ReceiveMessageRequest>()).Returns(x => GenerateEmptyMessage());
            _sqs.When(x => x.ReceiveMessage(Arg.Any<ReceiveMessageRequest>())).Do(x => _callCount++);
        }

        protected override void When()
        {
            SystemUnderTest.Listen();
        }

        [Then]
        public void ListenLoopDoesNotDie()
        {
            Patiently.AssertThat(() => _callCount > 3);
        }

        public override void PostAssertTeardown()
        {
            base.PostAssertTeardown();
            SystemUnderTest.StopListening();
        }

        private ReceiveMessageResponse GenerateEmptyMessage()
        {
            return new ReceiveMessageResponse { Messages = new List<Message>() };
        }
    }
}