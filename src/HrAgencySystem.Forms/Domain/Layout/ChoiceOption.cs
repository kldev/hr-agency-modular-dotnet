namespace HrAgencySystem.Forms.Domain.Layout;

/// <summary>
/// One answer a choice field offers. <see cref="Value"/> is what is stored and reported on,
/// <see cref="Label"/> is what people read - so a relabelled option does not split a report in two.
/// </summary>
public sealed record ChoiceOption(string Value, string Label);
