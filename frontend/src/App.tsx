import { Route, Routes } from "react-router-dom";
import { AppLayout } from "./components/layout/AppLayout";
import Stub from "./components/Stub";
import { ForgotPasswordPage } from "./features/auth/pages/ForgotPasswordPage";
import { LoginPage } from "./features/auth/pages/LoginPage";
import { ROUTES } from "./routes";

export const App: React.FC = () => {
	return (
		<Routes>
			<Route path={ROUTES.LOGIN} element={<LoginPage />} />

			<Route path={ROUTES.FORGOT_PASSWORD} element={<ForgotPasswordPage />} />

			<Route element={<AppLayout />}>
				<Route path={ROUTES.DASHBOARD} element={<Stub />} />

				<Route path={ROUTES.COMPANIES} element={<Stub />} />
				<Route path={ROUTES.JOBS} element={<Stub />} />
				<Route path={ROUTES.CANDIDATES} element={<Stub />} />
				<Route path={ROUTES.APPLICATIONS} element={<Stub />} />
				<Route path={ROUTES.SALES} element={<Stub />} />
				<Route path={ROUTES.SALES_OPPORTUNITIES} element={<Stub />} />
				<Route path={ROUTES.INTERVIEWS} element={<Stub />} />
				<Route path={ROUTES.CALENDAR} element={<Stub />} />
				<Route path={ROUTES.ORGANIZATIONS} element={<Stub />} />
				<Route path={ROUTES.SETTINGS} element={<Stub />} />
				<Route path={ROUTES.USERS} element={<Stub />} />
				<Route path={ROUTES.REPORTS} element={<Stub />} />
			</Route>
		</Routes>
	);
};
