using HrAgencySystem.Forms.Infrastructure.Configuration;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Forms;

/// <summary>
/// Forms, documents and surveys an administrator defines without a developer: pages of fields, a
/// catalogue of system fields shared by all of them, published versions and the responses filled in
/// for a person (plan 028).
/// <para>
/// Modelled softly on purpose, next to modules modelled hard. What has business rules is an
/// aggregate somewhere else; what is only content - with validation of that content - is a form.
/// A form never decides anything about a worker, and no other module decides anything by reading
/// a form.
/// </para>
/// <para>
/// It knows whose response it is only as an opaque <c>SubjectRef</c>; the worker is read through the
/// <c>IWorkerSnapshotRepository</c> port and never referenced.
/// </para>
/// </summary>
public static class FormsModule
{
    extension(IServiceCollection services)
    {
        public void AddFormsModule()
        {
            services.AddFormsServices();
        }
    }

    public static void ConfigureMarten(StoreOptions options)
    {
        options.ConfigureDocuments();
        options.ConfigureEvents();
        options.ConfigureProjections();
    }
}
