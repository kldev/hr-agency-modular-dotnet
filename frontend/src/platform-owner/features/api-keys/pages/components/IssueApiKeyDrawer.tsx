import { Check, Copy, TriangleAlert } from "lucide-react";
import { forwardRef, useImperativeHandle, useState } from "react";
import type { ServiceApiKeyIssued } from "#/api/models";
import { ApiError } from "#/components/ui/ApiError";
import { Button } from "#/components/ui/Button";
import { Drawer } from "#/components/ui/Drawer";
import { Input } from "#/components/ui/Input";
import { copyToClipboard } from "#/utlis/copyToClipboard";
import { useIssueServiceApiKey } from "../hooks";

export interface IssueApiKeyCommand {
	issue: () => void;
}

/** Mirrors `ServiceApiKey.NameMaxLength`. */
const nameMaxLength = 100;

/**
 * One field, then one moment that matters: the key's value is shown here and nowhere else, ever.
 * The API keeps only its hash, so closing this without copying it means issuing a new key.
 */
export const IssueApiKeyDrawer = forwardRef<IssueApiKeyCommand>((_, ref) => {
	const [open, setOpen] = useState(false);
	const [name, setName] = useState("");
	const [issued, setIssued] = useState<ServiceApiKeyIssued | null>(null);
	const [copied, setCopied] = useState(false);

	const mutation = useIssueServiceApiKey();

	useImperativeHandle(ref, () => ({
		issue: () => {
			mutation.reset();
			setName("");
			setIssued(null);
			setCopied(false);
			setOpen(true);
		},
	}));

	const trimmed = name.trim();
	const nameError =
		trimmed.length > nameMaxLength
			? `A key name cannot be longer than ${nameMaxLength} characters.`
			: null;

	const submit = () => {
		if (!trimmed || nameError) return;

		mutation.mutate(trimmed, { onSuccess: (result) => setIssued(result) });
	};

	const copy = async () => {
		if (!issued) return;

		await copyToClipboard(issued.value);
		setCopied(true);
	};

	const close = () => {
		if (mutation.isPending) return;

		setOpen(false);
	};

	return (
		<Drawer
			open={open}
			title={issued ? "Copy the key now" : "Issue an API key"}
			onClose={close}
			footer={
				issued ? (
					<Button variant="primary" onClick={close}>
						{copied ? "Done" : "I have stored it"}
					</Button>
				) : (
					<Button
						variant="primary"
						loading={mutation.isPending}
						disabled={!trimmed || Boolean(nameError)}
						onClick={submit}
					>
						Issue key
					</Button>
				)
			}
		>
			{issued ? (
				<div className="drawer-form">
					<div className="form-hint flex items-start gap-2">
						<TriangleAlert size={16} className="shrink-0 text-(--color-danger)" />
						<span>
							This is the only time the key is shown. The system keeps just its hash - lose it and
							the answer is a new key.
						</span>
					</div>

					<div className="form-field">
						<label className="form-label" htmlFor="api-key-value">
							{issued.name}
						</label>

						<div className="flex gap-2">
							<Input
								id="api-key-value"
								readOnly
								value={issued.value}
								className="font-mono"
								onFocus={(event) => event.currentTarget.select()}
							/>

							<Button
								icon={copied ? <Check size={15} /> : <Copy size={15} />}
								onClick={() => void copy()}
							>
								{copied ? "Copied" : "Copy"}
							</Button>
						</div>
					</div>

					<div className="form-hint">
						For the public job board: put it in <code>infrastructure/.env</code> as{" "}
						<code>WebApiKey</code> for the docker stack, or locally run{" "}
						<code>
							dotnet user-secrets set InternalApi:ApiKey &lt;key&gt; --project
							src/HrAgencySystem.Web
						</code>
						.
					</div>
				</div>
			) : (
				<form
					className="drawer-form"
					onSubmit={(event) => {
						event.preventDefault();
						submit();
					}}
				>
					<div className="form-field">
						<label className="form-label" htmlFor="api-key-name">
							What is it for
						</label>

						<Input
							id="api-key-name"
							value={name}
							placeholder="e.g. public job board"
							maxLength={nameMaxLength + 1}
							disabled={mutation.isPending}
							onChange={(event) => setName(event.target.value)}
						/>

						{nameError ? <span className="field-validation">{nameError}</span> : null}
					</div>

					<div className="form-hint">
						A key belongs to a program, not to an organization: the job board serves every agency
						and names the one it means in each call.
					</div>

					<ApiError error={mutation.error as unknown as Parameters<typeof ApiError>[0]["error"]} />
				</form>
			)}
		</Drawer>
	);
});

IssueApiKeyDrawer.displayName = "IssueApiKeyDrawer";
