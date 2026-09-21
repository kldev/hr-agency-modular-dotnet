namespace HrAgencySystem.Agency.Domain;

/// <summary>
/// What a unit is called in conversation. Descriptive and nothing more: no rule anywhere reads it,
/// because the tree already says what a unit is - a section is a section because it hangs under a
/// department, not because it carries a label saying so.
/// </summary>
public enum OrgUnitKind
{
    Board = 0,
    Department = 1,
    Section = 2,
}
