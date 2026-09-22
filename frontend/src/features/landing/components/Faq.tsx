import { ChevronDown } from "lucide-react";
import { faq } from "../content";

export function Faq() {
	return (
		<section className="landing-section">
			<div className="landing-container landing-faq-layout">
				<h2>Questions agencies ask first</h2>

				<div className="landing-faq">
					{faq.map((item) => (
						<details key={item.question}>
							<summary>
								{item.question}
								<ChevronDown size={18} aria-hidden="true" />
							</summary>
							<p>{item.answer}</p>
						</details>
					))}
				</div>
			</div>
		</section>
	);
}
