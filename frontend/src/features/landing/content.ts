/*
 * Everything the landing page says, kept apart from the markup so the copy can be checked against
 * the product in one place. Each claim here is something the panel does today: channels are
 * recorded rather than published to, and compliance covers the two countries the catalogue holds.
 */

export const navLinks = [
	{ href: "#features", label: "Features" },
	{ href: "#job-board", label: "Job board" },
	{ href: "#compliance", label: "Compliance" },
	{ href: "#how", label: "How it works" },
	{ href: "#contact", label: "Contact" },
] as const;

export interface Module {
	name: string;
	description: string;
}

export interface Pillar {
	title: string;
	summary: string;
	modules: Module[];
}

export const pillars: Pillar[] = [
	{
		title: "Recruiting for your clients",
		summary: "From the first sales call to a signed offer.",
		modules: [
			{
				name: "Sales pipeline",
				description:
					"Opportunities from first contact to won or lost, with follow-ups and history.",
			},
			{
				name: "Companies and contacts",
				description: "Your clients and the people you deal with there.",
			},
			{
				name: "Job descriptions",
				description: "One internal brief per position, from draft to closed.",
			},
			{
				name: "Job posts",
				description:
					"Candidate-facing copy in several languages, and a record of every board it went to.",
			},
			{
				name: "Candidates and applications",
				description: "One file per candidate, each application moved stage by stage to hired.",
			},
			{
				name: "Interviews and calendar",
				description: "HR, technical, client and final rounds, online, on site or by phone.",
			},
		],
	},
	{
		title: "Delivering what you sold",
		summary: "The projects, contracts and people you place with a client.",
		modules: [
			{
				name: "Projects and contracts",
				description:
					"A project goes live with a signed contract, a responsible contact and a complete client profile.",
			},
			{
				name: "Workers",
				description:
					"One file per person: identity, documents, permits and their path to employment.",
			},
			{
				name: "Assignments",
				description:
					"Every posting is its own record, so a move to a new project keeps the history.",
			},
			{
				name: "Positions",
				description: "Every role across every project in one register.",
			},
			{
				name: "Legal entities",
				description: "The companies you trade and post people through.",
			},
		],
	},
	{
		title: "Running your own team",
		summary: "Your agency as an employer, not only as a supplier.",
		modules: [
			{
				name: "Org chart",
				description: "Boards, departments and sections. Supervisors follow from the chart.",
			},
			{
				name: "Employment",
				description: "Contract type, period, weekly hours and rate for each of your people.",
			},
			{
				name: "Time sheets",
				description:
					"One sheet per person per month, approved by their supervisor and settled by payroll.",
			},
			{
				name: "Recruitment teams",
				description: "Who recruits with whom, grouped the way you work.",
			},
		],
	},
];

export const channels = [
	"Career page",
	"Pracuj.pl",
	"JustJoinIt",
	"NoFluffJobs",
	"RocketJobs",
	"LinkedIn",
	"Indeed",
	"OLX",
	"Praca.pl",
];

export interface ComplianceCell {
	count: number;
	example: string;
}

export const engagementTypes = [
	"Posting of workers",
	"Outsourcing",
	"Temporary agency work",
	"Local employment",
] as const;

/** Counts mirror `ComplianceCatalogue` on the backend, one row per country it holds. */
export const complianceRows: { country: string; code: string; cells: ComplianceCell[] }[] = [
	{
		country: "Germany",
		code: "DE",
		cells: [
			{ count: 6, example: "§ 18 AEntG notification" },
			{ count: 7, example: "Service contract delimitation" },
			{ count: 10, example: "AÜG hiring-out permit" },
			{ count: 2, example: "Social security registration" },
		],
	},
	{
		country: "Belgium",
		code: "BE",
		cells: [
			{ count: 6, example: "Limosa declaration" },
			{ count: 7, example: "Prohibited placement check" },
			{ count: 9, example: "Temporary agency recognition" },
			{ count: 2, example: "Dimona declaration" },
		],
	},
];

export const steps = [
	{
		title: "Leave your details",
		description:
			"Your name, your agency and an e-mail address or phone number. That is all we need.",
	},
	{
		title: "We set up your agency",
		description:
			"Your organization, the address of your job board, the e-mail domains your team uses and the first admin account.",
	},
	{
		title: "Your team signs in",
		description:
			"The admin adds colleagues and gives them roles. Your job board goes live with the first published post.",
	},
];

export const faq = [
	{
		question: "Can I create an account on my own?",
		answer:
			"Not yet. We set up each agency with you: its name, the address of its job board and the e-mail domains your team signs in with. Leave your details and our sales team will arrange it.",
	},
	{
		question: "How does my team sign in?",
		answer:
			"With their work e-mail address. The domain tells the system which agency they belong to, so nobody has to pick it from a list.",
	},
	{
		question: "Do you publish our posts to JustJoinIt or NoFluffJobs?",
		answer:
			"No. You record which boards each post went to, and your open posts are published as XML and JSON feeds that a board or partner can read.",
	},
	{
		question: "Is our data kept apart from other agencies?",
		answer:
			"Yes. Every record belongs to exactly one agency, and so does every stored document. A file from another agency is not found at all.",
	},
	{
		question: "Which countries does compliance cover?",
		answer:
			"Germany and Belgium, for posting, outsourcing, temporary agency work and local employment. Posting within Poland raises no host country duties, so there is nothing to track there.",
	},
];

export const teamSizes = ["1–5", "6–20", "21–50", "More than 50"] as const;
