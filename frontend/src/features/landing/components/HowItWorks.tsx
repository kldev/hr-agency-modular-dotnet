import { steps } from "../content";

export function HowItWorks() {
	return (
		<section id="how" className="landing-section landing-section-alt">
			<div className="landing-container">
				<header className="landing-section-header">
					<h2>How you get started</h2>
					<p>There is no sign-up form. We set your agency up together with you.</p>
				</header>

				<ol className="landing-steps">
					{steps.map((step) => (
						<li key={step.title}>
							<h3>{step.title}</h3>
							<p>{step.description}</p>
						</li>
					))}
				</ol>
			</div>
		</section>
	);
}
