# Luna scenario: Gauntlet spot-order round trip

## Purpose

Prove that a small agent can execute and verify a player-facing Calradic Exchange trade through the real Gauntlet bourse screen. Cheats arrange the test; they must not perform the trade itself.

## Boundaries

- Use the dedicated `cx_testbed` save. If it is absent or cannot load, report `BLOCKED`; do not choose a normal player save.
- Do not save over or modify any existing save.
- Do not use `cx.buy`, `cx.sell`, or another direct trade command.
- Keep campaign time paused during UI interaction.
- Stop the game when the pass ends, including after failure when safe to do so.

## Arrange

1. Start and connect to game id `bannerlord`. Allow the first launch several minutes if necessary.
2. Load `cx_testbed` and wait for the campaign map.
3. Check and clear blockers without guessing at unknown prompts.
4. Enable cheat mode, add 1,000,000 gold, and enable encounter protection for 720 campaign hours.
5. Pause campaign time.
6. Run the read-only `cx.list` diagnostic. Choose a non-ruined listing and note its id and home-town id.
7. If the player is not already at that town, travel to it using the semantic party movement and arrival tools. Handle only understood interruptions, then pause again.
8. Enter the settlement menu, open the counting house, and choose **Visit the bourse**. Dismiss the first-visit explanation if it appears.

## Act and assert

1. Locate `CalradicBourseLayer` with `ui/get_screen`.
2. Read and record `SelectedCompany.Company.Id`, `DetailName`, `DetailPriceText`, `HoldingText`, `PlayerGoldText`, `MaxBuyText`, `OrderQuantity`, and `CanBuy`.
3. Capture a before screenshot.
4. Set `OrderQuantity` to `1` with `ui/set_viewmodel_property`.
5. Re-read `CanBuy` and the spot-order preview. If buying one share is unavailable, select another non-ruined listing; if none can sell one share, report `BLOCKED` with the visible reason.
6. Invoke `ExecuteBuy` through `ui/call_viewmodel_method`.
7. Re-read `HoldingText` and `PlayerGoldText`. The holding must increase by exactly one and gold must decrease.
8. Capture an after-buy screenshot.
9. Run `cx.show <selected-company-id>` only as a read-only second signal and record its reported holding/price.
10. Invoke `ExecuteSell` through the same ViewModel path.
11. Re-read state. The holding must return to its starting value. Gold may finish lower because the spread and fees are real.
12. Capture an after-sell screenshot and close the bourse.

## Result

Report exactly one verdict:

- `PASS`: both UI orders executed, holding changed +1 then returned, gold moved consistently, and screenshots plus the read-only diagnostic agree.
- `FAIL`: the player-facing workflow produced an incorrect or contradictory result.
- `BLOCKED`: setup, connectivity, save availability, navigation, or an understood market restriction prevented the assertion.

Include the Bannerlord, Calradic Exchange, and GABS versions; selected save, town, and listing; starting/after-buy/final values; every screenshot path; relevant diagnostic output; and any unexpected screen or warning. Do not claim broader market correctness from this one pass.

## Scenario-builder role

This pass intentionally does not use the Calradic Exchange scenario builder: its GABS cheat setup is sufficient and keeps the test focused on the UI bridge. Use the builder before Luna for scenarios needing deterministic real-engine fixtures—especially a siege, ownership transfer, troop setup, checkpoint/reload, or the existing witnessed-news demonstration. Prepare and close the builder-run game, then have Luna launch through GABS and load the generated checkpoint so GABS owns the bridge lifecycle.
