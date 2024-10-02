using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateWPFSample.SharedCode
{
    internal class SampleNavigationParameters
    {
        private bool shouldWaitForCompletion;

        public CancellationToken CancellationToken { get; private set; }
        public string ModelPath => @"D:\New folder\microsoft--Phi-3-mini-4k-instruct-onnx\main\cpu_and_mobile\cpu-int4-rtn-block-32";
        public HardwareAccelerator HardwareAccelerator => HardwareAccelerator.CPU;
        public LlmPromptTemplate? PromptTemplate { get; } =
            new LlmPromptTemplate
            {
                System = "<|system|>\n{{CONTENT}}<|end|>\n",
                User = "<|user|>\n{{CONTENT}}<|end|>\n",
                Assistant = "<|assistant|>\n{{CONTENT}}<|end|>\n",
                Stop = ["<|system|>", "<|user|>", "<|assistant|>", "<|end|>"]
            };
        public bool ShouldWaitForCompletion
        {
            [MemberNotNullWhen(true, nameof(SampleLoadedCompletionSource))]
            get
            {
                return shouldWaitForCompletion;
            }
        }

        public TaskCompletionSource<bool>? SampleLoadedCompletionSource { get; set; }

        public SampleNavigationParameters(CancellationToken loadingCanceledToken)
        {
            CancellationToken = loadingCanceledToken;
        }

        public void RequestWaitForCompletion()
        {
            shouldWaitForCompletion = true;
            SampleLoadedCompletionSource = new TaskCompletionSource<bool>();
        }

        public void NotifyCompletion()
        {
            SampleLoadedCompletionSource?.SetResult(true);
        }
    }
}