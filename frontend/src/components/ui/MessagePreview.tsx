import { useState } from "react";
import { Dialog } from "./Dialog";

interface MessagePreviewProps {
	message: string;
	maxLength?: number;
}

export function MessagePreview({ message, maxLength = 100 }: MessagePreviewProps) {
	const [open, setOpen] = useState(false);

	const isLong = message.length > maxLength;
	const preview = isLong ? `${message.slice(0, maxLength)}...` : message;

	return (
		<>
			<div className="max-w-full">
				<p className="whitespace-pre-wrap wrap-break-word text-sm text-(--color-text)">{preview}</p>

				{isLong && (
					<button
						type="button"
						onClick={() => setOpen(true)}
						className="mt-1 cursor-pointer text-sm font-medium text-(--color-info)! hover:underline"
					>
						Show more
					</button>
				)}
			</div>

			<Dialog open={open} title="Message" onClose={() => setOpen(false)} maxWidth="lg">
				<div className="whitespace-pre-wrap wrap-break-word text-sm leading-6 text-(--color-text)">
					{message}
				</div>
			</Dialog>
		</>
	);
}
