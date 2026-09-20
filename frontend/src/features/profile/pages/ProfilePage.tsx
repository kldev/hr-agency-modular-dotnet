import { DetailsLoading } from "#/components/ui";
import { DetailOverviewHeader } from "#/components/ui/details/DataDetails";
import { AvatarUploader } from "../components/AvatarUploader";
import { PasswordForm } from "../components/PasswordForm";
import { ProfileForm } from "../components/ProfileForm";
import { useGetOwnProfile } from "./hooks";
import "./profile.css";

/**
 * Three things a person can do about themselves, each submitted on its own. Deliberately not one
 * form: changing a phone number and changing a password have nothing to do with each other, and the
 * second one ends the session.
 */
export function ProfilePage() {
	const query = useGetOwnProfile();

	if (query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id="me" isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const profile = query.data;

	return (
		<div className="profile-page">
			<header className="profile-header">
				<h1>My profile</h1>
				<p>Your details, your picture and your password.</p>
			</header>

			<section className="data-details-section">
				<div className="data-overview">
					<DetailOverviewHeader
						title="Picture"
						description="Shown next to your name in the application."
					/>

					<AvatarUploader profile={profile} />
				</div>
			</section>

			<section className="data-details-section">
				<div className="data-overview">
					<DetailOverviewHeader title="Details" description="How colleagues can reach you." />

					<ProfileForm profile={profile} />
				</div>
			</section>

			<section className="data-details-section">
				<div className="data-overview">
					<DetailOverviewHeader
						title="Password"
						description="Changing it signs you out everywhere, including here."
					/>

					<PasswordForm />
				</div>
			</section>
		</div>
	);
}
