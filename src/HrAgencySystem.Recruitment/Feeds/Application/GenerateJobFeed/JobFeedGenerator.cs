using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using HrAgencySystem.Recruitment.Config;
using HrAgencySystem.Recruitment.Feeds.Application.GetJobFeed;
using HrAgencySystem.Recruitment.Feeds.ReadModel;
using HrAgencySystem.Recruitment.Feeds.Serialization;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Recruitment.Feeds.Application.GenerateJobFeed;

// ReSharper disable once ClassNeverInstantiated.Global
internal class JobFeedGenerator(
    IJobFeedReader reader,
    IOptions<RecruitmentConfig> config) : IJobFeedGenerator
{
    public async Task<JobFeedContent> GenerateAsync(Guid organizationId, CancellationToken ct)
    {
        var feedUrl = config.Value.FeedUrl;

        if (string.IsNullOrEmpty(feedUrl))
            throw new ArgumentException(
                "FeedsUrl must be provided. Check AppSettings.json -> Application -> FeedUrl value.");

        var jobs = await reader.GetJobsFeed(organizationId, ct);

        var json = SerializeJson(jobs, feedUrl);
        var xml = SerializeXml(jobs, feedUrl);
        return new JobFeedContent(json, xml);
    }

    internal static string SerializeJson(IReadOnlyList<JobPostFeedRow> jobs, string feedUrl)
    {
        var options = JsonSerializerOptions.Web;
        var jsonFeed = new JobFeedJson()
        {
            Jobs = [.. jobs.Select(job => JobJson.FromRow(job, feedUrl))]
        };
        
        var json = JsonSerializer.Serialize(jsonFeed, options);
        return json;
    }

    internal static string SerializeXml(
        IReadOnlyList<JobPostFeedRow> jobs, string feedUrl)
    {
        var serializer = new XmlSerializer(
            typeof(JobFeedXml));

        var feed = new JobFeedXml
        {
            Jobs = [.. jobs.Select(job => JobFeedXmlItem.FromRow(job, feedUrl))]
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