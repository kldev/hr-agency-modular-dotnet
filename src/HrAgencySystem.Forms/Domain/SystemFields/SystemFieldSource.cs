using HrAgencySystem.Forms.Domain.Layout;

namespace HrAgencySystem.Forms.Domain.SystemFields;

/// <summary>
/// Where a system field can be pre-filled from when nobody has answered it yet. A closed list on
/// purpose: each entry is a line of mapping onto <c>WorkerSnapshot</c>, and the worker's file is a
/// hard model owned by another module. A system field with no source - PESEL, a bank account - is
/// defined by the administrator alone.
/// <para>Only ever read from. Nothing a form collects is written back to the worker's file.</para>
/// <para>Append only: Marten stores the ordinal.</para>
/// </summary>
public enum SystemFieldSource
{
    None,
    WorkerFirstName,
    WorkerLastName,
    WorkerDateOfBirth,
    WorkerCitizenship,
    WorkerEmail,
    WorkerPhone,
}

public static class SystemFieldSources
{
    extension(SystemFieldSource source)
    {
        /// <summary>The type a value from this source has; a source is only allowed on a field of that type.</summary>
        public FieldType? ValueType =>
            source switch
            {
                SystemFieldSource.WorkerFirstName or SystemFieldSource.WorkerLastName => FieldType.Text,
                SystemFieldSource.WorkerDateOfBirth => FieldType.Date,
                SystemFieldSource.WorkerCitizenship => FieldType.Country,
                SystemFieldSource.WorkerEmail => FieldType.Email,
                SystemFieldSource.WorkerPhone => FieldType.Phone,
                _ => null,
            };
    }
}
