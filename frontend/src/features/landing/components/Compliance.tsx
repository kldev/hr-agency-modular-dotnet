import { complianceRows, engagementTypes } from "../content";

export function Compliance() {
	return (
		<section id="compliance" className="landing-section">
			<div className="landing-container">
				<header className="landing-section-header">
					<h2>The paperwork depends on how you send someone</h2>
					<p>
						Posting an IT specialist to Germany asks for little. Hiring the same person out to a
						German client asks for a permit, a notification and documents kept on site. The
						checklist follows the country and the form of engagement together, and certificates like
						A1 are tracked per person, per posting.
					</p>
				</header>

				<div className="landing-matrix-scroll">
					<table className="landing-matrix">
						<caption className="landing-sr-only">
							Duties tracked per country and form of engagement
						</caption>
						<thead>
							<tr>
								<th scope="col">Country</th>
								{engagementTypes.map((type) => (
									<th key={type} scope="col">
										{type}
									</th>
								))}
							</tr>
						</thead>
						<tbody>
							{complianceRows.map((row) => {
								const heaviest = Math.max(...row.cells.map((cell) => cell.count));
								return (
									<tr key={row.code}>
										<th scope="row">
											<span className="landing-matrix-code">{row.code}</span>
											{row.country}
										</th>
										{row.cells.map((cell, index) => (
											<td
												key={engagementTypes[index]}
												className={cell.count === heaviest ? "landing-matrix-heaviest" : undefined}
											>
												<strong>{cell.count}</strong>
												<span>{cell.count === 1 ? "duty" : "duties"}</span>
												<small>incl. {cell.example}</small>
											</td>
										))}
									</tr>
								);
							})}
						</tbody>
					</table>
				</div>

				<p className="landing-footnote">
					Posting people within Poland raises no host country duties, so there is nothing to track
					there.
				</p>
			</div>
		</section>
	);
}
