import { FileSpreadsheet } from "lucide-react";

interface ExportButtonProps {
	href: string;
}

/**
 * A plain link, like the settlement export: the proxy adds the credential, the browser saves the
 * file the way it saves any download, and nothing is held in memory.
 */
export function ExportButton({ href }: ExportButtonProps) {
	return (
		<a className="button button-secondary" href={href} download>
			<FileSpreadsheet size={14} />
			Export to Excel
		</a>
	);
}
