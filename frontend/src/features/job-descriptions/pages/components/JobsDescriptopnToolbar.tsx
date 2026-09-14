import { useRouter } from "@tanstack/react-router";
import { Plus, Search, SlidersHorizontal } from "lucide-react";
import { Button } from "@/components/ui";
import { Input } from "@/components/ui/Input";

interface JobsPageToolbarProps {
	search: string;
	onSearchChange: (value: string) => void;
	onClear: () => void;
}

export function JobsDescriptopnToolbar({ search, onSearchChange, onClear }: JobsPageToolbarProps) {
	const router = useRouter();
	const addUrl = router.buildLocation({ to: "/app/job-descriptions/add" }).href;
	const handleAdd = () => {
		window.open(addUrl, "_blank", "noopener,noreferrer");
	};

	return (
		<div className="toolbar">
			<div className="toolbar-left">
				<span className="search-input">
					<Search
						size={15}
						aria-hidden="true"
						style={{
							position: "absolute",
							left: 12,
							top: 10,
						}}
					/>

					<span className="sr-only">Search job posts</span>

					<Input
						value={search}
						placeholder="Search job posts"
						onChange={(event) => onSearchChange(event.target.value)}
					/>
				</span>

				<Button
					variant="ghost"
					icon={<SlidersHorizontal size={15} />}
					aria-label="Clear"
					title="Clear"
					onClick={onClear}
				>
					Clear
				</Button>
			</div>

			<div className="toolbar-right">
				<Button variant="primary" icon={<Plus size={15} />} onClick={handleAdd}>
					Add job description
				</Button>
			</div>
		</div>
	);
}
