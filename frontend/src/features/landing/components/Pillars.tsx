import { pillars } from "../content";

export function Pillars() {
	return (
		<section id="features" className="landing-section">
			<div className="landing-container">
				<header className="landing-section-header">
					<h2>Three jobs, one workspace</h2>
					<p>
						An agency sells a search, delivers the people it found and employs a team of its own.
						Each of those has its own screens, so none of them is squeezed into another.
					</p>
				</header>

				<div className="landing-pillars">
					{pillars.map((pillar) => (
						<article key={pillar.title} className="landing-pillar">
							<h3>{pillar.title}</h3>
							<p className="landing-pillar-summary">{pillar.summary}</p>

							<dl className="landing-modules">
								{pillar.modules.map((module) => (
									<div key={module.name}>
										<dt>{module.name}</dt>
										<dd>{module.description}</dd>
									</div>
								))}
							</dl>
						</article>
					))}
				</div>
			</div>
		</section>
	);
}
