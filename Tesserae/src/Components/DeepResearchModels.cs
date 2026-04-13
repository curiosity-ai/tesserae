using System;
using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.DeepResearchStatus")]
    public enum DeepResearchStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Canceled
    }

    [H5.Name("tss.DeepResearchPlanModel")]
    public sealed class DeepResearchPlanModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DeepResearchStatus Status { get; set; }
        public float? Progress { get; set; }
        public string CurrentStage { get; set; }
        public IList<DeepResearchStepModel> Steps { get; set; } = new List<DeepResearchStepModel>();
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string FooterMessage { get; set; }
    }

    [H5.Name("tss.DeepResearchStepModel")]
    public sealed class DeepResearchStepModel
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public DeepResearchStatus Status { get; set; }
        public float? Progress { get; set; }
        public string CurrentStage { get; set; }
        public IList<DeepResearchSubstepModel> Substeps { get; set; } = new List<DeepResearchSubstepModel>();
        public bool? IsExpanded { get; set; }
    }

    [H5.Name("tss.DeepResearchSubstepModel")]
    public sealed class DeepResearchSubstepModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DeepResearchStatus Status { get; set; }
        public string Message { get; set; }
    }
}
