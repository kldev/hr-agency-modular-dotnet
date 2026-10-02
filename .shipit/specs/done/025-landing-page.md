# 025 · Landing page

**Status:** done (disabled by default) · **Built:** 2026-09-22 → 2026-09-23 · **Verify:** frontend only, no automated check · **Plan:** frontend 013 (local, not in git)

## Why
The platform owner needs a public page that tells recruitment agencies what the product is and
invites them to get in touch. Only the platform owner can create an organization - there is no
self-service sign-up - so the call to action is "leave your details and our sales team will contact
you", not "create an account".

## What it promises
- `/` renders a single long public page: a sticky top bar with anchor links, Sign in (`/login`) and
  Book a demo (`#contact`); below 900px the links fold into a menu that closes on navigation.
- Sections: hero with a CSS mock of the applications pipeline, the three axes of the product
  (recruiting for a client, delivering the service, the agency as an employer), the public job board
  and XML/JSON feeds with an XML excerpt, compliance per country and engagement type, how it works
  in three steps, an FAQ built from `<details>`, the contact form, a footer.
- The copy promises only what the code does: channels are recorded, not published to; a won deal does
  not create a project by itself; compliance covers two countries; no pricing, client logos or quotes.
- The contact form requires a name and an agency name, and at least one of e-mail or phone; a
  malformed e-mail or a phone with fewer than 7 digits is refused with a named message. Team size and
  message are optional.
- Sending shows a short loading state and then a thank-you naming the channel we would use; a note
  says it is a demo and nothing is saved or sent.
- The page sets its own `<title>` and meta description.
- The route redirects to `/login` unless the environment variable `LANDING=1` is set.

## Surface
- Frontend: `frontend/src/routes/index.tsx`, `frontend/src/features/landing/` (`LandingPage.tsx`,
  `content.ts`, `landing.css`, `components/*`, `components/useContactForm.ts`).
- Backend: none.
- Tests: none.

## Out of scope for this feature
- A leads endpoint and a mail to sales - not yet; the form sends nothing.
- Self-service organization sign-up - not ever as part of this page (owner creates organizations).
- Redirecting a signed-in user from `/` to the panel - not yet.
- Pricing, testimonials, client logos - not ever without real ones.

## Acceptance criteria
- [ ] The page renders with all sections and anchor navigation — unproven (no e2e or screenshot found)
- [ ] Contact form validation (name, agency, e-mail or phone, formats) — unproven (no test found; `validateContact` is a pure function with no Vitest case)
- [ ] The demo note and thank-you state — unproven (no test found)
- [ ] `/` redirects to `/login` unless `LANDING=1` — unproven (no test found)

## Decisions
- A hand-written `<form>` with `useState`, like the sign-in and forgot-password pages - public pages do
  not use TanStack Form: [ADR-0022](../../../docs/adr/0022-frontend-tanstack-start-orval.md) for the stack.
- Content (sections, FAQ) is kept as data in `content.ts`; styles are scoped with a `landing-` prefix.
- No backend involvement at all, so the page cannot break or be broken by the API.

## History
| Date | Commit | Change |
|---|---|---|
| 2026-09-22 | `969647f4` | Public landing page with demo contact form |
| 2026-09-23 | `b3eb61d5` | Landing disabled: `/` redirects to `/login` unless `LANDING=1` |
| 2026-09-23 | `c1826663` | Mobile menu closes on hash change |
| 2026-10-01 | `5891f80f` | Contact form buttons moved to HeroUI |

## Notes from reconstruction
- The plan listed a "Modules" grid section; no such component was ever committed.
- The disable switch reads `process.env.LANDING` inside the route's `beforeLoad`. The Vite config
  defines no replacement for it, so in client-side navigation it is probably `undefined` and only a
  server render can honour `LANDING=1` - not verified.
- The plan was "to be accepted" (status line) and was implemented without a follow-up note.
