import { channels } from "../content";

// The element names are the ones `JobFeedXml` writes; the values are an example.
const FEED_SAMPLE = `<jobs>
  <job>
    <title>Senior .NET developer</title>
    <location>Kraków</location>
    <employmentType>Contract</employmentType>
    <workMode>Hybrid</workMode>
    <currencyCode>PLN</currencyCode>
    <salaryMin>22000</salaryMin>
    <salaryMax>28000</salaryMax>
    <skills>
      <skill>C#</skill>
      <skill>PostgreSQL</skill>
    </skills>
    <applyUrl>…/your-agency/senior-net-developer</applyUrl>
  </job>
</jobs>`;

export function BoardAndFeeds() {
	return (
		<section id="job-board" className="landing-section landing-section-alt">
			<div className="landing-container landing-split">
				<div className="landing-split-copy">
					<h2>Your own job board, and feeds a board can read</h2>

					<p>
						Every agency gets a public board at its own address. A candidate who applies there lands
						in your pipeline as an application, with nothing to retype.
					</p>

					<p>
						Your open posts are also written to <code>jobs.xml</code> and <code>jobs.json</code> and
						refreshed every couple of minutes, so a partner can pull them instead of waiting for a
						copy.
					</p>

					<h3>Where each post went</h3>
					<p>
						Record which boards a post was published on, so the team can see at a glance what is
						live where.
					</p>
					<ul className="landing-channels">
						{channels.map((channel) => (
							<li key={channel}>{channel}</li>
						))}
					</ul>
				</div>

				<figure className="landing-code">
					<figcaption>jobs.xml</figcaption>
					<pre>
						<code>{FEED_SAMPLE}</code>
					</pre>
				</figure>
			</div>
		</section>
	);
}
