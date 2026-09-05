using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;
using HrAgencySystem.Recruitment.Feeds.Serialization;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
internal class JobFeedGenerator(IJobFeedReader reader) : IJobFeedGenerator
{
    public async Task<JobFeedContent> GenerateAsync(Guid organizationId, CancellationToken ct)
    {
        var jobs = await reader.GetJobsFeed(organizationId, ct);

        var json = SerializeJson(jobs);
        var xml = SerializeXml(jobs);
        return new JobFeedContent(json, xml);
    }

    private static string SerializeJson(IReadOnlyList<JobPostProjection> jobs)
    {
        var options = JsonSerializerOptions.Web;
        var jsonFeed = new JobFeedJson()
        {
            Jobs = [.. jobs.Select(JobJson.FromProjection)]
        };
        
        var json = JsonSerializer.Serialize(jsonFeed, options);
        return json;
    }

    private static string SerializeXml(
        IReadOnlyList<JobPostProjection> jobs)
    {
        var serializer = new XmlSerializer(
            typeof(JobFeedXml));

        var feed = new JobFeedXml
        {
            Jobs = [.. jobs.Select(JobFeedXmlItem.FromProjection)]
        };

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();

        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, feed);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
    
    internal sealed record JobFeedContent(
        string Json,
        string Xml);
}