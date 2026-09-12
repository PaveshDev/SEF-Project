import { PartnerDirectory } from './pages/PartnerDirectory';
import { AcceptanceRuleEditor } from './pages/AcceptanceRuleEditor';
import { RecipientNeeds } from './pages/RecipientNeeds';
import { MatchReview } from './pages/MatchReview';
import { UnmatchedItems } from './pages/UnmatchedItems';

export const partnersRoutes = [
  { path: 'partners', element: <PartnerDirectory /> },
  { path: 'partners/rules', element: <AcceptanceRuleEditor /> },
  { path: 'partners/needs', element: <RecipientNeeds /> },
  { path: 'partners/matches', element: <MatchReview /> },
  { path: 'partners/unmatched', element: <UnmatchedItems /> }
];
