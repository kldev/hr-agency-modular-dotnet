import { expect, type Locator, type Page } from "@playwright/test";
import { fillDate } from "./ui";

/**
 * A real, one page PDF built in memory, so uploads go through the file service with a file any
 * viewer opens - and no binary has to live in the repository. Every document is marked as a
 * specimen: they carry made-up names and must never pass for the real thing.
 */
export function specimenPdf(title: string, lines: string[] = []): Buffer {
	const pdfText = (text: string) => text.replace(/[\\()]/g, (c) => `\\${c}`);

	const text = [
		"BT /F1 20 Tf 72 760 Td",
		`(${pdfText(title)}) Tj`,
		"/F1 11 Tf 0 -28 Td",
		...lines.flatMap((line) => [`(${pdfText(line)}) Tj`, "0 -16 Td"]),
		"0 -24 Td (SPECIMEN - generated for automated tests, not a real document) Tj",
		"ET",
	].join("\n");

	const objects = [
		"<< /Type /Catalog /Pages 2 0 R >>",
		"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
		"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
		`<< /Length ${Buffer.byteLength(text)} >>\nstream\n${text}\nendstream`,
		"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
	];

	let pdf = "%PDF-1.4\n";
	const offsets: number[] = [];

	objects.forEach((body, index) => {
		offsets.push(Buffer.byteLength(pdf));
		pdf += `${index + 1} 0 obj\n${body}\nendobj\n`;
	});

	const xref = Buffer.byteLength(pdf);
	pdf += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n`;
	pdf += offsets.map((offset) => `${String(offset).padStart(10, "0")} 00000 n \n`).join("");
	pdf += `trailer\n<< /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;

	return Buffer.from(pdf, "latin1");
}

export type DocumentUpload = {
	fileName: string;
	title: string;
	lines?: string[];
	category: string;
	documentDate?: string;
	validUntil?: string;
	note?: string;
};

/**
 * Attaches one document through the "Attach document" drawer - the same path a person takes - and
 * waits for it to be listed. The worker's tab opens it with "+" (Add), the project's with a button
 * named after the drawer.
 */
export async function attachDocument(page: Page, tab: Locator, document: DocumentUpload) {
	await tab.getByRole("button", { name: /^(Add|Attach document)$/ }).click();

	const drawer = page.getByRole("dialog", { name: "Attach document" });
	await drawer.getByLabel(/Choose a file/).setInputFiles({
		name: document.fileName,
		mimeType: "application/pdf",
		buffer: specimenPdf(document.title, document.lines),
	});
	await expect(drawer.getByText(document.fileName)).toBeVisible();

	await drawer.getByLabel("Category").selectOption({ label: document.category });

	if (document.documentDate) {
		await fillDate(drawer, "Document date", document.documentDate);
	}

	if (document.validUntil) {
		await fillDate(drawer, "Valid until", document.validUntil);
	}

	if (document.note) {
		await drawer.getByLabel("Note").fill(document.note);
	}

	await drawer.getByRole("button", { name: "Save changes" }).click();
	await expect(drawer).toBeHidden();
	await expect(tab.getByText(document.fileName)).toBeVisible();
}
