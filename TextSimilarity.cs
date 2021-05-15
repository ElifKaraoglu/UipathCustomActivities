using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using TextSimilarity.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace TextSimilarity.Activities
{
    [LocalizedDisplayName(nameof(Resources.TextSimilarity_DisplayName))]
    [LocalizedDescription(nameof(Resources.TextSimilarity_Description))]
    public class TextSimilarity : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.TextSimilarity_String1_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextSimilarity_String1_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> String1 { get; set; }

        [LocalizedDisplayName(nameof(Resources.TextSimilarity_String2_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextSimilarity_String2_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> String2 { get; set; }

        [LocalizedDisplayName(nameof(Resources.TextSimilarity_Percentage_DisplayName))]
        [LocalizedDescription(nameof(Resources.TextSimilarity_Percentage_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<double> Percentage { get; set; }

        #endregion


        #region Constructors

        public TextSimilarity()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (String1 == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(String1)));
            if (String2 == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(String2)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var string1 = String1.Get(context);
            var string2 = String2.Get(context);

            double in_percent = new double();
            string a = string1;
            string b = string2;
            char[] turkishChars = { 'ý', 'ð', 'Ý', 'Ð', 'ç', 'Ç', 'þ', 'Þ', 'ö', 'Ö', 'ü', 'Ü' };
            char[] englishChars = { 'i', 'g', 'I', 'G', 'c', 'C', 's', 'S', 'o', 'O', 'u', 'U' };
            // Match char
            for (int i = 0; i < turkishChars.Length; i++)
            {
                a = a.Replace(turkishChars[i], englishChars[i]);
                b = b.Replace(turkishChars[i], englishChars[i]);
            }
            a = a.ToLower().ToUpper();
            b = b.ToLower().ToUpper();



            if (string.IsNullOrEmpty(a))
            {
                in_percent = b.Length;
            }
            if (string.IsNullOrEmpty(b))
            {
                in_percent = a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;
            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1), distances[i - 1, j - 1] + cost);
                }
                in_percent = distances[lengthA, lengthB];
                in_percent = (1.0 - ((double)in_percent / (double)Math.Max(a.Length, b.Length))) * 100;
            }

            // Outputs
            return (ctx) => {
                Percentage.Set(ctx, in_percent);
            };
        }

        #endregion
    }
}

