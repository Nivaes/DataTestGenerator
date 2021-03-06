﻿using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nivaes.DataTestGenerator.Xunit
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    public class RetryTestCase
        : XunitTestCase
    {
        private int mMaxRetries;

        private int mTimeSleep;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public RetryTestCase() { }

        public RetryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod,
            int maxRetries, int timeSleep)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments: null)
        {
            mMaxRetries = maxRetries;
            mTimeSleep = timeSleep;
        }

        // This method is called by the xUnit test framework classes to run the test case. We will do the
        // loop here, forwarding on to the implementation in XunitTestCase to do the heavy lifting. We will
        // continue to re-run the test until the aggregator has an error (meaning that some internal error
        // condition happened), or the test runs without failure, or we've hit the maximum number of tries.
        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            if (diagnosticMessageSink == null) throw new ArgumentNullException(nameof(diagnosticMessageSink));
            if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));

            var runCount = 0;

            while (true)
            {
                // This is really the only tricky bit: we need to capture and delay messages (since those will
                // contain run status) until we know we've decided to accept the final result;
                var delayedMessageBus = new DelayedMessageBus(messageBus);

                var summary = await base.RunAsync(diagnosticMessageSink, delayedMessageBus, constructorArguments, aggregator, cancellationTokenSource).ConfigureAwait(false);
                if (aggregator.HasExceptions || summary.Failed == 0 || ++runCount >= mMaxRetries)
                {
                    delayedMessageBus.Dispose();  // Sends all the delayed messages

                    return summary;
                }

                await Task.Delay(mTimeSleep).ConfigureAwait(false);

                diagnosticMessageSink.OnMessage(new DiagnosticMessage("Execution of '{0}' failed (attempt #{1}), retrying...", DisplayName, runCount));
            }
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            base.Serialize(data);

            data.AddValue("MaxRetries", mMaxRetries);
            data.AddValue("TimeSleep", mTimeSleep);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            base.Deserialize(data);

            mMaxRetries = data.GetValue<int>("MaxRetries");
            mTimeSleep = data.GetValue<int>("TimeSleep");
        }
    }
}
