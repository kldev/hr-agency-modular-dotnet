import clsx from "clsx";
import { Paperclip, UploadCloud, X } from "lucide-react";
import { type DragEvent, useId, useRef, useState } from "react";
import { formatFileSize } from "#/utlis";
import "./file-dropzone.css";

interface FileDropzoneProps {
	/** What the browser's own picker should offer, e.g. ".png,.jpg". */
	accept?: string;

	/** One line under the prompt saying what is accepted and how large it may be. */
	hint?: string;

	label?: string;

	file?: File | null;

	error?: string | null;

	/** 0-100 while an upload is in flight, null when nothing is being sent. */
	progress?: number | null;

	disabled?: boolean;

	compact?: boolean;

	onSelect: (file: File | null) => void;
}

/**
 * The one place a file is picked. A bare `<input type="file">` renders as "Choose File / No file
 * chosen" - a control most people have to look for twice and which gives no hint about what is
 * accepted - so this is a target you can also drop a file onto.
 *
 * The input itself stays the real control, only visually hidden, and the whole panel is its
 * `<label>`: clicking anywhere in the zone opens the picker natively, and the field stays in the
 * tab order for anyone who is not using a mouse.
 */
export function FileDropzone({
	accept,
	hint,
	label = "File",
	file = null,
	error = null,
	progress = null,
	disabled = false,
	compact = false,
	onSelect,
}: FileDropzoneProps) {
	const inputId = useId();

	const [dragging, setDragging] = useState(false);

	/*
	 * Dragging over a child fires dragleave on the parent, which would make the highlight flicker.
	 * Counting enter/leave pairs instead of trusting a single event keeps it steady.
	 */
	const depth = useRef(0);

	const stopDragging = () => {
		depth.current = 0;
		setDragging(false);
	};

	const handleDragEnter = (event: DragEvent<HTMLLabelElement>) => {
		event.preventDefault();

		if (disabled) return;

		depth.current += 1;
		setDragging(true);
	};

	const handleDragLeave = (event: DragEvent<HTMLLabelElement>) => {
		event.preventDefault();

		depth.current -= 1;

		if (depth.current <= 0) {
			stopDragging();
		}
	};

	// Without this the browser opens the file instead of letting the page have it.
	const handleDragOver = (event: DragEvent<HTMLLabelElement>) => {
		event.preventDefault();
	};

	const handleDrop = (event: DragEvent<HTMLLabelElement>) => {
		event.preventDefault();

		stopDragging();

		if (disabled) return;

		const dropped = event.dataTransfer.files?.[0];

		// Only ever one file: every caller here owns a single document or a single picture.
		if (dropped) {
			onSelect(dropped);
		}
	};

	return (
		<div className="file-dropzone-field">
			{label ? (
				<span className="form-label" id={`${inputId}-label`}>
					{label}
				</span>
			) : null}

			<label
				className={clsx(
					"file-dropzone",
					compact && "file-dropzone-compact",
					dragging && "is-dragging",
					disabled && "is-disabled",
				)}
				onDragEnter={handleDragEnter}
				onDragLeave={handleDragLeave}
				onDragOver={handleDragOver}
				onDrop={handleDrop}
			>
				<input
					id={inputId}
					type="file"
					className="file-dropzone-input"
					accept={accept}
					disabled={disabled}
					onChange={(event) => {
						onSelect(event.target.files?.[0] ?? null);

						// Picking the same file twice in a row must still fire a change event.
						event.target.value = "";
					}}
				/>

				<UploadCloud size={compact ? 18 : 24} className="file-dropzone-icon" />

				<span className="file-dropzone-title">
					<span className="file-dropzone-action">Choose a file</span> or drag it here
				</span>

				{hint ? <span className="file-dropzone-hint">{hint}</span> : null}
			</label>

			{file ? (
				<div className="file-dropzone-selected">
					<Paperclip size={15} className="file-dropzone-icon shrink-0" />

					<span className="file-dropzone-selected-name">{file.name}</span>

					<span className="file-dropzone-selected-size">{formatFileSize(file.size)}</span>

					<button
						type="button"
						className="action-button shrink-0"
						title="Remove the chosen file"
						aria-label="Remove the chosen file"
						disabled={disabled}
						onClick={() => onSelect(null)}
					>
						<X size={15} />
					</button>
				</div>
			) : null}

			{progress !== null ? (
				<div className="file-dropzone-progress">
					<div className="file-dropzone-progress-bar" style={{ width: `${progress}%` }} />
				</div>
			) : null}

			{error ? <p className="form-error">{error}</p> : null}
		</div>
	);
}
