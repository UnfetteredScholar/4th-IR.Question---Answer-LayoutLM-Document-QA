using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using _4thIR.QAnswerLA.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using QuestionAnswering.ImageContext;

namespace _4thIR.QAnswerLA.Activities
{
    [LocalizedDisplayName(nameof(Resources.QuestionAnswering_DisplayName))]
    [LocalizedDescription(nameof(Resources.QuestionAnswering_Description))]
    public class QuestionAnswering : ContinuableAsyncCodeActivity
    {
        private static readonly ImageQuestionAnswerer _answerer = new ImageQuestionAnswerer();
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.Timeout_DisplayName))]
        [LocalizedDescription(nameof(Resources.Timeout_Description))]
        public InArgument<int> TimeoutMS { get; set; } = 60000;

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_ImageContextPath_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_ImageContextPath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> ImageContextPath { get; set; }

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_Question_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_Question_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Question { get; set; }

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_Answer_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_Answer_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> Answer { get; set; }

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_Score_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_Score_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<double> Score { get; set; }

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_Start_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_Start_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<int> Start { get; set; }

        [LocalizedDisplayName(nameof(Resources.QuestionAnswering_End_DisplayName))]
        [LocalizedDescription(nameof(Resources.QuestionAnswering_End_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<int> End { get; set; }

        #endregion


        #region Constructors

        public QuestionAnswering()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ImageContextPath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ImageContextPath)));
            if (Question == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(Question)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var timeout = TimeoutMS.Get(context);
           

            // Set a timeout on the execution
            var task = ExecuteWithTimeout(context, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) != task) throw new TimeoutException(Resources.Timeout_Error);

            // Outputs
            return (ctx) => {
                Answer.Set(ctx, task.Result.answer);
                Score.Set(ctx, task.Result.score);
                Start.Set(ctx, task.Result.start);
                End.Set(ctx, task.Result.end);
            };
        }

        private async Task<(string answer, double score, int start, int end)> ExecuteWithTimeout(AsyncCodeActivityContext context, CancellationToken cancellationToken = default)
        {
            var imageContextPath = ImageContextPath.Get(context);
            var question = Question.Get(context);
            var res = await _answerer.AnswerQuestion(imageContextPath, question);

            return res;
        }

        #endregion
    }
}

