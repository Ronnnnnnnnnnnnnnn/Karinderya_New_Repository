# Karinderya — Gameplay & Systems Documentation

**A cozy Filipino karinderya management game**

Version 1.0 · November 24, 2026 · Unity 6 · Windows · Single-player

---

## At a glance

Karinderya is a first-person/third-person 3D management game about running a small Filipino village food stall. The player farms vegetables, cooks familiar dishes, stocks buffet trays, serves lunch and dinner customers, completes a small set of daily requests, and closes the stall to advance to the next morning.

The design is built around a single daily rhythm:

1. **Prepare** — plant, water, harvest, cook, and stock buffets.
2. **Serve** — handle the lunch and dinner rushes.
3. **Improve** — complete requests, earn trust, unlock recipes and regulars.
4. **Close** — review the day, pay the stall fee, save, and continue tomorrow.

| Area | Implemented in this build |
|---|---|
| Farming | 16 active plots, 9 crops, watering, varied growth times, regrowable crops, harvest and save state |
| Cooking | 5 dishes, unlockable recipes, ingredient checks, pot cooking timers |
| Buffets | Per-dish trays, **4 servings per cooked dish**, visual serving count, save state |
| Serving | 3 service counters, customer orders, patience, tips, angry-customer penalties |
| Rushes | Lunch 11:00–14:00 and dinner 17:00–21:00, different caps, faster arrivals, +25% pay |
| Customers | Boy, girl, old man, old woman color variants; regular customers after 40 trust |
| Requests | Two daily requests from a physical board, event-driven progress, expiry, day summary |
| Trust | 0–100 relationship score, recipe rewards, regulars, catering, milestone feedback |
| Economy | Seed prices, recipe unlocks, daily stall fee, rush/regular bonuses, compensation |
| Day/night | Clock-driven sun rotation, ambient light, dusk/night colors, screen tint |
| Save/Continue | JSON save, auto-load, periodic autosave, save on quit/menu/day close, farm and kitchen state |
| HUD | Clock, rush countdown, active request count, trust, farm timers, notifications |
| Presentation | Scene-authored request board, notebook, day summary card, HUD strip |

---

## 1. Game Overview

### Player fantasy

You own a modest karinderya at the edge of a Filipino village. Behind the stall is a small garden. Inside are pots, ingredient pickups, and buffet trays. Customers arrive from the village paths, take a counter, and wait for a familiar dish at a fair price.

The game is not about building a restaurant empire in one sitting. It is about the comfortable, repeatable work of feeding a neighborhood: knowing what to plant, preparing before the rush, and earning enough trust that the village keeps coming back.

### What the player does

- Plants and waters crops, then harvests ingredients.
- Cooks dishes from recipes at the pots.
- Places cooked dishes on matching buffet trays.
- Takes individual servings and serves waiting customers.
- Reads daily requests from a physical request board.
- Manages seed costs, recipe unlock costs, and the daily stall fee.
- Builds village trust to unlock recipes, regulars, and catering.
- Closes the stall, reads the day summary, and continues the next morning.

### Design pillars

**Filipino village warmth.** Tagalog dish names, karinderya food, village customers, and a small neighborhood business instead of a generic restaurant.

**Prep, then rush.** Mornings are for farming and cooking. Lunch and dinner are short, busy service windows where preparation pays off.

**Daily requests.** The request board gives the day shape without turning the game into a giant quest log.

**Visible growth.** Coins, village trust, regulars, more recipes, and a more confident karinderya over time.

---

## 2. Quick Start & Controls

### Controls

| Input | Action |
|---|---|
| W A S D | Move |
| Mouse | Look / camera |
| Left click | Interact with the object under the crosshair |
| E | Context action: read the board, use a pot/buffet, pick up, water, plant, harvest |
| I | Open or close inventory |
| B | Open or close seed shop |
| Esc | Close the notebook or open the pause menu |
| F1 | Skip the tutorial |

### First five minutes

1. Play the tutorial, or press **F1** to skip it.
2. Walk to the **REQUESTS** board at the front of the karinderya.
3. Look at the board and press **E** to open the request notebook.
4. Check the day, time, trust, and two active requests.
5. Go to the farm, till a plot, buy a seed, plant it, and water it.
6. Cook a dish at a pot, place it in the matching buffet, and take a serving.
7. Serve a customer by looking at them and left-clicking while holding the correct serving.
8. Open the board again and click **Close Stall / End Day** when customers and crops are settled.

### Where everything is

| Place | Purpose |
|---|---|
| Front of the karinderya | Request board, customer counters, serving area |
| Kitchen | Pots, buffets, ingredient pickups, cooking |
| Farm behind the karinderya | 16 active crop plots |
| Seed shop | Buy seeds with coins |
| Request notebook | Read requests, check trust, close the day |

---

## 3. The Day Loop

A full day runs from 6:00 AM to the next morning. At the default clock speed, one game minute passes per real second, so a full 24-hour cycle is about 24 real minutes.

| Phase | Time | Customer cap | Notes |
|---|---|---|---|
| Dawn / morning | 6:00–11:00 | 1–2 | Prep time. Normal arrival delays. |
| Lunch rush | 11:00–14:00 | 2 | Faster arrivals, +25% customer pay. |
| Afternoon | 14:00–17:00 | 1–2 | Restock, farm, finish requests. |
| Dinner rush | 17:00–21:00 | 3 | Largest crowd, +25% customer pay. |
| Late evening | 21:00–22:00 | 1–2 | Closing window. |
| Late night | 22:00–6:00 | 1 | Slow traffic. Dark world. |

### Closing the stall

- Open the request notebook and click **Close Stall / End Day**.
- Closing is blocked while customers are still waiting.
- Closing is blocked while crops are in **Seed** or **Seedling** state.
- Active requests are completed if their conditions are already met; otherwise they are missed.
- Missed requests cost 1 trust.
- The daily stall fee is deducted at close.
- A **Day Complete** card appears with requests completed, net customer/mission coins, and trust gained.
- The clock advances to 6:00 AM and new requests are posted.
- Progress is saved after the day closes.

**Daily stall fee:** ₱25 per completed day. The fee is charged at close and included in the day summary.

---

## 4. Farming

Farming is the ingredient supply chain. Meat comes from a reusable kitchen pickup; vegetables come from the farm.

### Farm loop

1. Buy a seed from the shop. Seeds cost ₱10–₱25 each.
2. Equip the hoe and till a soil plot into farmland.
3. Equip the seed and plant it.
4. Equip the watering can and water the plot.
5. Wait for the crop to grow. Water lasts 3 game hours.
6. Harvest the ripe crop by looking at it and pressing E or left-clicking.
7. Use the ingredient at a pot, or let a regrowable crop reset for another harvest.

### Farm size

The scene now contains **16 authored farm plots** arranged as a deliberate garden. The intent is a small, readable farm where each seed choice and watering decision matters, rather than a mass-production field.

### Crop reference

| Crop | Grow time | Real time at 1× | Regrow | Seed price | Used by |
|---|---|---|---|---|---|
| Carrot | 1.5 game hours | 90 sec | No | ₱10 | Kaldereta, Giniling |
| Okra | 1.5 game hours | 90 sec | 1.5 h | ₱12 | Pakbet, Dinengdeng |
| Potato | 3 game hours | 180 sec | No | ₱15 | Kaldereta, Giniling |
| Labanos | 3 game hours | 180 sec | No | ₱15 | Sinigang |
| Sitaw | 3 game hours | 180 sec | 3 h | ₱15 | Sinigang |
| Tomato | 3 game hours | 180 sec | 3 h | ₱15 | Sinigang |
| Ampalaya | 4.5 game hours | 270 sec | No | ₱20 | Pakbet |
| Talong | 4.5 game hours | 270 sec | 3 h | ₱20 | Pakbet |
| Kalabasa | 6 game hours | 360 sec | No | ₱25 | Pakbet, Dinengdeng |

### Watering and wilting

- Water lasts 3 game hours.
- While watered, the crop grows using the game clock.
- If the plot dries before the crop matures, growth stops and the crop begins to wilt.
- Slow crops such as Kalabasa and Ampalaya need a second watering to reach harvest.
- A mature crop stays harvestable, but it can still wilt if left dry too long.
- The farm HUD shows the first few active crops and their remaining time.

**Day-end safety:** the Close Stall button will not skip past Seed or Seedling crops. Harvest them or clear them with the shovel before ending the day.

### Why farming matters

Vegetables are the limiting ingredient for later recipes. The first two dishes can be cooked from starting carrots and potatoes, but Pakbet, Dinengdeng, and Sinigang require farmed crops.

---

## 5. Cooking & Dishes

Cooking turns ingredients into a whole dish. The dish is then placed in a buffet, which portions it into individual servings for customers.

### Cooking loop

1. Hold the required ingredients and approach an idle pot.
2. Open the recipe book with E or left-click.
3. Select a recipe and press **Start Cooking**.
4. Wait for the cooking timer.
5. Take the cooked dish from the pot.
6. Place the dish in its matching buffet.

### Recipe reference

| Dish | Ingredients | Cook time | Selling price | Unlock cost |
|---|---|---|---|---|
| Kaldereta | Meat + Carrot + Potato | 10 sec | ₱60 | Free |
| Giniling | Meat + Carrot + Potato | 10 sec | ₱60 | Free |
| Pakbet | Okra + Ampalaya + Kalabasa + Talong | 15 sec | ₱75 | ₱500 |
| Dinengdeng | Meat + Okra + Kalabasa | 20 sec | ₱85 | ₱1,000 |
| Sinigang | Meat + Sitaw + Labanos + Tomato | 30 sec | ₱95 | ₱1,500 |

**Meat is reusable.** The kitchen meat pickup is an infinite ingredient source. Farming is therefore about vegetables, not meat.

### Recipe unlocks

- Kaldereta and Giniling are available from the start.
- Pakbet, Dinengdeng, and Sinigang can be purchased at their buffet stations.
- Trust rewards can also unlock Pakbet, Dinengdeng, and Sinigang for free.
- If a trust reward recipe was already purchased, the player receives coin compensation instead.

---

## 6. Buffets & Servings

A cooked dish is not served directly. It is placed on a buffet tray, which turns it into individual servings.

### How a buffet works

1. Hold the finished dish.
2. Look at the buffet that matches the dish.
3. Press E to place the dish. The tray fills with **4 servings**.
4. Press E again to take one serving into the hand.
5. Serve the customer while holding that serving.

Each buffet holds only its own recipe. A Kaldereta dish cannot go into the Sinigang tray. The buffet visibly counts down its remaining servings.

**Economy effect:** one cooked dish now produces 4 servings. That makes ingredient costs, recipe unlocks, and the daily stall fee matter without making the loop grindy.

### Why buffets matter

- They let the player batch-prepare before a rush.
- They convert one cooking action into several customer transactions.
- They create a visible stock target: "Do I have enough servings for dinner?"
- They limit the benefit of over-cooking because trays run out.

---

## 7. Customers & Rushes

Customers arrive from the village-side entrances and take one of the three service counters. The player can serve them in any order.

### Customer flow

1. A customer spawns from a random front entrance.
2. They walk to a free counter.
3. Their order appears above their head with a patience timer.
4. The player prepares or takes the correct serving.
5. Look at the customer and left-click to serve.
6. The customer pays, leaves a notification, and walks away.

### Patience

- Standard patience is 60 seconds.
- The tutorial customer receives extra patience.
- The order label turns yellow at 50% patience and red at 25%.
- An angry customer costs compensation and one point of village trust, once per day.

### Rush hours

| Rush | Window | Max customers | Bonus |
|---|---|---|---|
| Lunch | 11:00–14:00 | 2 | +25% customer pay |
| Dinner | 17:00–21:00 | 3 | +25% customer pay |

Off-peak hours usually have one customer, with a chance of a second. Late night has one customer and much slower arrivals.

### Customer types

| Type | Color | Spawn weight |
|---|---|---|
| Boy | Light blue | 30% |
| Girl | Pink | 30% |
| Old man | Slate gray | 20% |
| Old woman | Lavender | 20% |

Only the character mesh is tinted. The order label and dish icon stay readable. The angry red tint overrides the type color when a customer leaves upset.

### Three counters, no queue

The wide front of the karinderya uses three service spots. If all three are occupied, no new customer spawns until one leaves. There is no separate waiting line yet; the counters are the queue.

---

## 8. Regular Customers

At **40 village trust**, the karinderya starts attracting regulars.

- Each new customer has a **25% chance** to be a regular after the unlock.
- Regulars pay **25% more**.
- Regulars have **15 extra seconds** of patience.
- Their order label is marked with a leading `*`.
- The arrival notification says "Regular At Counter!"
- Serving a regular shows "(Regular)" in the coin notification.

This is the first version of the regular system. Named regulars with standing orders and friendship progress are planned for a later content pass.

---

## 9. Requests & Missions

The request board is the mission system. It is intentionally diegetic: the player walks to a physical board and reads a notebook instead of staring at a permanent quest log.

### What a request is

A request is a small, day-scoped goal posted by a villager or group. It always has:

- A requester and a short description.
- One objective: serve customers, serve a specific dish, earn customer coins, or keep a clean shift.
- A time window or day-long condition.
- A coin reward and a trust reward.
- A visible status: In Progress, Complete, or Missed.

Requests are **not** a generic quest log. They exist to give the day a shape and to make the player choose what to prepare.

### How requests work

- Two requests are active each business day.
- Requests are generated at 6:00 AM or after the tutorial.
- Progress is event-driven: the game listens for successful customer transactions.
- Requests complete automatically and pay coins and trust.
- A missed time window or unfinished day request expires and costs 1 trust.
- There is no game-over state; a missed request is a setback, not a restart.

### Request rotation

| Day cycle | Request 1 | Request 2 |
|---|---|---|
| Cycle 0 (Days 1, 4, 7…) | **Lunch Crowd** — serve 2 customers, 11:00–14:00. ₱120, +5 trust | **Villager Request** — serve 1 specific unlocked dish. ₱150, +6 trust |
| Cycle 1 (Days 2, 5, 8…) | **Dinner Full House** — serve 3 customers, 17:00–21:00. ₱200, +8 trust | **Market Money** — earn ₱250 from customers. ₱100, +5 trust |
| Cycle 2 (Days 3, 6, 9…) | **Clean Shift** below 60 trust — serve at least 1 customer with no angry customers. ₱120, +8 trust. **Barangay Catering** at 60+ trust — serve 5 customers by 21:00. ₱500, +15 trust | **Villager Request** — serve 1 specific unlocked dish. ₱150, +6 trust |

If no suitable dish is available, the dish request falls back to a customer-earnings request. Duplicate request IDs are skipped.

### The notebook

- Shows the day, time, and village trust.
- Shows two request cards with requester, description, progress, time window, reward, and status.
- Completed requests are marked COMPLETE; expired requests are marked MISSED.
- The footer explains why closing is blocked: waiting customers or growing crops.
- The notebook pauses gameplay time while open.

### Day summary

After closing the stall, a summary card appears over the request notebook:

> **DAY COMPLETE**
>
> Day 3 complete — 2/2 requests · ₱420 · +8 trust

The card is the player's proof that the day moved forward. It is not a separate menu system; it sits on top of the request notebook. It reports:

- The day that was completed.
- Requests completed out of the day's total.
- Net customer and mission coins, minus the stall fee.
- Trust gained from requests.
- The stall fee when one was charged.

Press **Continue** to dismiss the card and begin the new morning.

---

## 10. Trust & Progression

Village trust is the long-term relationship score. It rises when the player completes requests and falls when customers leave angry or requests are missed.

| Trust | Reward | Meaning |
|---|---|---|
| Start | 10 trust | The village is willing to give the new stall a chance. |
| 20 | Pakbet recipe free, or ₱200 if already owned | The village starts trusting your cooking. |
| 40 | Regular customers unlocked | The stall has repeat visitors. |
| 60 | Dinengdeng recipe free, or ₱300 if already owned; Barangay Catering can appear | The karinderya is becoming a village institution. |
| 80 | Sinigang recipe free, or ₱400 if already owned | The village trusts you with its signature dishes. |
| 100 | Maximum trust | Future festival and ending content can use this ceiling. |

### Trust changes

- Completing a request: + the request's trust reward.
- An angry customer: −1 trust, once per day.
- An expired or missed request: −1 trust.
- Trust is clamped between 0 and 100.

**Design intent:** trust should be visible and useful. It is not just a score. It changes the recipe pool, the customer pool, and the mission pool.

---

## 11. Economy

The economy is built around preparation costs, serving revenue, recipe unlocks, and a small daily upkeep.

### Starting state

| Item | Value |
|---|---|
| Starting coins | ₱100 |
| Starting vegetables | 2 Carrots, 2 Potatoes |
| Meat | Infinite reusable kitchen pickup |
| Farm plots | 16 active plots |
| Daily stall fee | ₱25 per completed day |

### Money in

- Customer payments: ₱60–₱95 per serving, plus rush and regular bonuses.
- Request rewards: ₱100–₱500.
- Trust fallback rewards when a recipe was already purchased.

### Money out

- Seeds: ₱10–₱25 each.
- Recipe unlocks: ₱500 Pakbet, ₱1,000 Dinengdeng, ₱1,500 Sinigang.
- Daily stall fee: ₱25 per day.
- Angry customers: compensation deducted from coins.

### Revenue example

A Kaldereta dish costs vegetables to grow and produces 4 servings at ₱60 each, or ₱240 before rush bonuses. A dinner regular at rush time pays more, while a missed day still charges the stall fee. The player is encouraged to stock the right dishes rather than cook endlessly.

**Balance note:** the economy is intentionally tighter than the earlier prototype, but it is still tunable. Seed prices, serving capacity, recipe costs, mission rewards, and the daily fee are all data values in the project.

---

## 12. Save & Continue

The game is designed to be played across real days. It auto-loads the last save and keeps the day, farm, kitchen, and mission state.

### Save behavior

| Event | Save? |
|---|---|
| Game starts | Automatically loads the save if one exists |
| Every 20 seconds | Autosave |
| Quit application | Save |
| Application pause | Save |
| Return to main menu | Save |
| Close Stall / End Day | Save after the new day begins |
| Tutorial complete or skipped | Save requested |

### How the save system works

- `SaveManager` is the central save controller.
- On save, `SaveManager.Capture()` asks the gameplay systems to write their state into a serializable `SaveData` object.
- The object is converted to readable JSON with `JsonUtility.ToJson(data, true)`.
- The JSON is written to a single file under `Application.persistentDataPath`.
- On startup, `SaveManager.Preload()` reads the file before the gameplay scene loads.
- Each manager then restores its own slice of the data, including time, coins, inventory, recipes, farm plots, crops, buffets, and missions.
- Autosave runs every 20 seconds, and explicit saves happen on quit, pause, main menu, tutorial completion, and day close.

### Where the save file lives

The runtime path is:

```text
Application.persistentDataPath/karinderya_save.json
```

On Windows, with the project's current company/product settings, the full path is:

```text
%USERPROFILE%\AppData\LocalLow\DefaultCompany\Karinderya\karinderya_save.json
```

To open the folder, press **Win + R** and paste:

```text
%USERPROFILE%\AppData\LocalLow\DefaultCompany\Karinderya
```

The file is plain-text JSON and uses one save slot. It is created after the first save. Delete it to start a fresh game.

### What is saved

- Day, time, season, and clock speed.
- Coins and total dishes served.
- Tutorial completion.
- Unlocked recipes and claimed trust rewards.
- Inventory slots and equipped tool/item.
- Active 16 farm plots, soil state, and last watering time.
- Planted crops, growth, health, and crop state.
- Buffet contents and servings.
- Mission state, progress, reputation, day counters, and last day summary.

### What is not saved

- Customers currently walking, waiting, or leaving.
- The player's exact position and camera rotation.
- Open menus and UI state.
- Notifications currently in the queue.

**Save file:** `Application.persistentDataPath/karinderya_save.json`

There is one save slot. To start a fresh game, quit the game and delete this file. A New Game button is a planned menu addition.

### Continue flow

1. Launch the game and enter the gameplay scene.
2. The save is preloaded before the scene starts.
3. If the tutorial was completed, it is skipped automatically.
4. The clock loads at the saved day and time.
5. Inventory, farm, buffets, recipes, and missions load.
6. The player continues at the same day and farm state.

---

## 13. HUD & UI Guide

| UI element | Location | Purpose |
|---|---|---|
| Clock and date | Top center | Current time, day, season, day of week |
| Rush / request strip | Below the clock | Lunch/dinner countdown, active request count, village trust |
| Farm timer panel | Top right | Active crops and remaining time; hides when no crops |
| Status bar | Bottom left | Equipped tool and inventory shortcuts |
| Notifications | Bottom center | Arrivals, rewards, trust changes, missed requests |
| Request board | Front of the karinderya | Physical board with a warm light and "E to read" prompt |
| Request notebook | Opened from board | Request cards, progress, trust, day summary, Close Stall |
| Customer order label | Above customer | Dish name, patience timer, regular marker, warning colors |
| Interaction prompt | World / center | Context hint for interacting with an object |

### HUD philosophy

- The player should always know the time, the next rush, and whether requests are active.
- The HUD should not become a permanent quest log.
- Requests live on the board, but a small active-request count is always visible.
- Farm information appears only when crops exist and is hidden during menus.
- World-space labels should never cover each other; customer labels are staggered by counter.

**Current UI style:** warm paper, wood, cream, gold, and harvest green. The request notebook and day summary use a dark paper/ledger look. The long-term goal is to replace the remaining flat prototype panels with rounded paper and wood treatments.

---

## 14. Time & Day/Night

The clock is the single source of truth for customer phases, rush windows, missions, crop growth, and visual lighting.

### Clock rules

- The game starts on Day 1 at 6:00 AM.
- At `TimeManager.timeScale = 1`, one game minute passes per real second.
- A full 24-hour day takes about 24 real minutes.
- Days advance at midnight; the business day rolls over at 6:00 AM.
- The clock drives lunch, dinner, and late-night customer behavior.

### Day/night lighting

| Period | Hours | Visual |
|---|---|---|
| Night | 19:00–5:00 | Dim blue light, dark screen tint |
| Dawn | 5:00–7:00 | Warm low sun, fading tint |
| Day | 7:00–17:00 | Bright, warm directional light |
| Dusk | 17:00–19:00 | Orange sun, fading light |

The directional light rotates across the sky, changes color and intensity, ambient light dims at night, and a full-screen tint sits below the HUD so the clock, notebook, and notifications remain readable.

### Demo speed

The `TimeManager.timeScale` field can be raised in the Inspector to reach lunch or dinner faster. This speeds up the clock, day/night cycle, crop growth, and mission windows. Customer spawn/patience uses real time, so 3–4× is a good demo speed; 10× makes the rushes too short.

---

## 15. Technical Architecture

### Main systems

| System | Files | Responsibility |
|---|---|---|
| Clock | `Time Manager.cs`, `Game Timestamp.cs`, `ITimeTracker.cs` | Game time, day/season rollover, listener notifications |
| Day/night | `DayNightCycle.cs` | Sun rotation, light color/intensity, ambient, night tint |
| Customers | `CustomerSpawner.cs`, `CustomerAI.cs`, `CustomerOrder.cs`, `CustomerAppearance.cs` | Arrivals, counter assignment, orders, patience, service, type colors, regulars |
| Missions | `MissionManager.cs`, `MissionDefinition.cs` | Daily generation, progress, trust, rewards, day summary, save data |
| Missions UI | `MissionBoard.cs`, `MissionNotebookUI.cs`, `MissionCardView.cs` | Scene-authored physical board, notebook cards, day summary overlay |
| Farming | `LandManager.cs`, `Land.cs`, `CropBehaviour.cs`, `RegrowableHarvestBehaviour.cs` | 16 active plots, clock growth, water, harvest, regrow, save state |
| Cooking | `Pot.cs`, `CookingUIManager.cs`, `RecipeData.cs`, `RecipeUnlockManager.cs` | Recipe selection, ingredient checks, cooking timer, unlocks |
| Buffets | `BuffetContainer.cs` | Dish storage, 4 servings, serving retrieval, save state |
| Economy | `CurrencyManager.cs`, `ShopManager.cs`, `SeedData.cs` | Coins, seed purchases, customer/request rewards, daily fee |
| Save | `SaveManager.cs`, `SaveData.cs` | Preload, capture, JSON write, autosave, load hooks across systems |
| HUD | `HudStatusUI.cs`, `UIManager.cs`, `NotificationManager.cs`, `FarmTimerUIManager.cs` | Clock, rush/request strip, farm timers, notifications, menus |

### Event flow

```text
Customer served
    -> CustomerOrder fires CustomerFinished(success)
    -> MissionManager checks dish, coins, time window, and clean-shift state
    -> Coins, trust, notifications, and day-summary counters update
```

```text
Clock tick
    -> TimeManager notifies registered trackers every game minute
    -> Land grows crops, HUD refreshes, mission windows expire, day rolls over
    -> DayNightCycle reads the same timestamp for lighting
```

### Scene authoring

Core systems are authored in `PlayingScene` rather than spawned by bootstrap scripts:

- `Game Systems` holds `SaveManager`, `PlayerProgression`, `MissionManager`, and `TutorialManager`, with child objects for cooking, HUD, farm timer, and day/night control.
- `MissionBoard` is a placed world object with its frame, paper, text, light, and collider visible in the scene.
- `MissionNotebookCanvas` contains the blocker, notebook, request cards, buttons, and day-summary panel.
- `HUD`, `FarmTimerPanel`, `RecipeBookPanel`, and `IngredientPanel` are authored under the main canvas with their components and references assigned in the Inspector.
- `DayNightOverlay` and its `NightTint` image are scene objects.
- `TutorialCanvas` is scene-authored; tutorial path markers remain runtime gameplay indicators.

Runtime instantiation is limited to dynamic gameplay content such as customers, crops, servings, recipe rows, and notifications.

### Save architecture

- A JSON snapshot is written to the persistent data path.
- The snapshot is loaded before the gameplay scene starts.
- Each manager loads its own slice during Awake or Start.
- Autosave begins after the scene is ready.
- Farm saving waits for the active plot list to finish loading.
- End Day explicitly saves after the new day is generated.

### Data-driven tuning

| Want to change… | Where |
|---|---|
| Day length / demo speed | `TimeManager.timeScale` |
| Lunch/dinner hours | `CustomerSpawner` phase fields |
| Customer caps | `CustomerSpawner` phase max fields |
| Rush pay bonus | `CustomerSpawner.rushRewardMultiplier` |
| Regular chance/tip/patience | `MissionManager` regular fields |
| Crop growth and seed price | `SeedData` assets |
| Farm size | `LandManager.maxActivePlots` |
| Water duration | `Land.cs` clock check (3 hours) |
| Dish ingredients / price / unlock | `RecipeData` assets |
| Buffet servings | `BuffetContainer` maxServings in the scene |
| Mission pool and rewards | `MissionManager` builders |
| Trust thresholds | `MissionManager.CheckTrustRewards` |
| Daily stall fee | `MissionManager.dailyStallFee` |
| Autosave interval | `SaveManager.autosaveInterval` |

---

## 16. Current Limitations

These are the honest boundaries of the current build.

- One save slot. No cloud save or profile menu.
- No New Game button yet; delete the save file to reset.
- Customers in the scene are not saved; they respawn after load.
- Player position, camera, and open menus are not saved.
- No seven-day festival or ending yet.
- Catering is a large same-day order, not a multi-day catering contract.
- Regulars are probability-based customers, not named recurring characters yet.
- No staff, furniture upgrades, or decoration system yet.
- The farm uses 16 authored plots.
- Final balance and Play Mode QA still need a dedicated pass.
- UI uses a mix of legacy UI text and TextMeshPro; a full typography pass is planned.
- Localization, rebinding, and accessibility settings are not in yet.

### What this build is good for

- Demonstrating the core daily loop.
- Showing a diegetic mission system and trust progression.
- Showing integrated time, day/night, farming, customers, cooking, and saving.
- Showing a solid vertical-slice foundation for a longer game.

---

## 17. Roadmap

### Next priorities

1. **New Game / Continue menu** with a clear save-slot UI.
2. **Named regulars** with preferences and standing orders.
3. **Multi-day catering** and a seven-day festival arc.
4. **Upgrades and decorations**: more trays, a second pot, seating, paint, plants.
5. **Stronger UI art**: rounded paper, wood, icons, portraits, animated stamps.
6. **More recipes and crops**, including seasonal availability and market prices.
7. **More customer behavior**: small talk, preferences, patience by type.
8. **Full accessibility**: text scaling, colorblind-safe type markers, rebinding, localization.
9. **Balance and QA**: economy, crop timing, save edge cases, long-session testing.

### Long-term vision

A seven-day village festival arc in which the karinderya grows from a humble stall into the place where the whole barangay gathers. The current request board and trust system are the first layer of that arc.

---

## 18. Professor Demo Script

A focused 15–20 minute demonstration that shows the loop, the systems, and the technical structure without waiting through a full day.

1. **Start the game.** If a save exists, it auto-loads; if not, the tutorial starts.
2. **Skip the tutorial** with F1 for a clean demo.
3. **Open the request board.** Show the two requests, time, trust, and the notebook.
4. **Show the HUD.** Point out the clock, rush countdown, active requests, and trust.
5. **Raise `TimeManager.timeScale`** to 3–4 in the Inspector to reach lunch quickly.
6. **Show the farm.** Plant, water, and explain growth times and the 16-plot garden.
7. **Cook and stock.** Make a dish, place it in a buffet, show 4 servings.
8. **Serve lunch.** Show order labels, patience colors, rush pay, and mission completion.
9. **Open the board again.** Show request progress and the Close Stall button.
10. **End the day.** Show the day summary card, time jump, new requests, and autosave.
11. **Optional:** show a regular customer with the star marker after 40 trust.
12. **Optional:** show the day/night cycle by advancing time.

### What to say

> "The game is built around a single source of truth for time. The clock drives the day/night visuals, the lunch and dinner customer rushes, the mission windows, and crop growth. The request board is diegetic: the player reads a physical board, and a mission manager listens to the existing customer-finished event to update requests without polling. The save system preserves the day, farm, kitchen, buffets, inventory, recipes, and mission state across sessions."

### Features to highlight

- Event-driven mission tracking instead of a hard-coded quest loop.
- Time-of-day customer phases with different caps and rewards.
- Clock-driven farming with different crop durations and water management.
- Trust rewards that change the actual game systems.
- Automatic save/Continue for daily play.
- Scene-authored board, notebook, HUD, cooking panels, and day/night overlay, inspectable in the Unity editor.

---

## 19. QA & Tuning Checklist

### QA checklist

- [ ] Start with no save, finish or skip the tutorial, and confirm the tutorial does not repeat after restart.
- [ ] Plant and water a fast crop and a slow crop; confirm both mature with the correct timing.
- [ ] Close the day with a Seedling crop; confirm the Close Stall guard.
- [ ] Complete a request; restart; confirm progress and reward are preserved.
- [ ] Stock a buffet; restart; confirm the dish and servings remain.
- [ ] Unlock a recipe through trust and confirm the buffet updates.
- [ ] Reach 40 trust and confirm the regular star, tip, and patience bonus.
- [ ] Reach 60 trust and confirm the catering request appears on cycle 2.
- [ ] End a day and confirm the summary card, stall fee, and autosave.
- [ ] Delete the save file and confirm a clean new game.

### Quick tuning reference

| Value | Current |
|---|---|
| Starting trust | 10 |
| Maximum trust | 100 |
| Daily stall fee | ₱25 |
| Regular chance | 25% |
| Regular pay bonus | +25% |
| Regular patience bonus | +15 seconds |
| Lunch rush | 11:00–14:00 |
| Dinner rush | 17:00–21:00 |
| Lunch customer cap | 2 |
| Dinner customer cap | 3 |
| Active farm plots | 16 |
| Buffet servings | 4 |
| Customer patience | 60 seconds |
| Autosave interval | 20 seconds |

---

*Karinderya — Gameplay & Systems Documentation · Version 1.0 · November 24, 2026. This document describes the current playable vertical slice. Values may change during balance passes.*
