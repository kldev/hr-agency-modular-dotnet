/*
 * A drawing of the applications board, not a screenshot: the stages are the real ones
 * (`JobApplicationStatus`), the people are made up. One card moves from Interview to Offer once,
 * after the page loads - the only motion on the page that nobody asked for.
 */

interface Card {
	name: string;
	role: string;
	note: string;
}

const columns: { stage: string; cards: Card[] }[] = [
	{
		stage: "Applied",
		cards: [
			{ name: "Julia Nowak", role: "Senior .NET developer", note: "Career page" },
			{ name: "Tomasz Wiśniewski", role: "QA engineer", note: "JustJoinIt" },
			{ name: "Ola Kamińska", role: "React developer", note: "NoFluffJobs" },
		],
	},
	{
		stage: "Screening",
		cards: [
			{ name: "Piotr Zając", role: "DevOps engineer", note: "Call booked" },
			{ name: "Marta Lis", role: "Data engineer", note: "CV reviewed" },
		],
	},
	{
		stage: "Interview",
		cards: [
			{ name: "Kamil Wójcik", role: "Java developer", note: "Technical, Thu 10:00" },
			{ name: "Ewa Krawczyk", role: "Product designer", note: "Client, Fri 14:30" },
		],
	},
	{
		stage: "Offer",
		cards: [{ name: "Michał Pawlak", role: "Tech lead", note: "Offer sent" }],
	},
];

const moving: Card = {
	name: "Anna Kowalczyk",
	role: "Senior .NET developer",
	note: "Offer prepared",
};

function MockCard({ card, className }: { card: Card; className?: string }) {
	return (
		<div className={className ? `landing-mock-card ${className}` : "landing-mock-card"}>
			<span className="landing-mock-avatar">
				{card.name
					.split(" ")
					.map((part) => part[0])
					.join("")}
			</span>
			<span className="landing-mock-card-text">
				<strong>{card.name}</strong>
				<small>{card.role}</small>
				<em>{card.note}</em>
			</span>
		</div>
	);
}

function Count({ before, after }: { before: number; after: number }) {
	return (
		<span className="landing-mock-count">
			<span className="landing-mock-count-before">{before}</span>
			<span className="landing-mock-count-after">{after}</span>
		</span>
	);
}

export function PanelMockup() {
	return (
		<div className="landing-mock" aria-hidden="true">
			<div className="landing-mock-bar">
				<span className="landing-mock-dots">
					<i />
					<i />
					<i />
				</span>
				<span className="landing-mock-title">Applications</span>
				<span className="landing-mock-filter">Senior .NET developer</span>
			</div>

			<div className="landing-mock-board">
				{columns.map((column) => (
					<div key={column.stage} className="landing-mock-column">
						<div className="landing-mock-stage">
							{column.stage}
							{column.stage === "Interview" ? (
								<Count before={3} after={2} />
							) : column.stage === "Offer" ? (
								<Count before={1} after={2} />
							) : (
								<span className="landing-mock-count">{column.cards.length}</span>
							)}
						</div>

						{column.cards.map((card) => (
							<MockCard key={card.name} card={card} />
						))}

						{column.stage === "Interview" && <div className="landing-mock-slot" />}
						{column.stage === "Offer" && (
							<MockCard card={moving} className="landing-mock-card-moving" />
						)}
					</div>
				))}
			</div>
		</div>
	);
}
