# Stretch Reminder

A tiny Windows tray app that pops up every 45 minutes (configurable) with a
2–4 minute micro-session. Two programs, switchable from the window or tray:

- **Stretch** — built for one specific problem: **right-side neck stiffness
  with pain behind the right ear** — the classic upper-trap / levator
  scapulae / SCM / suboccipital pattern from desk work.
- **Weight loss** — bodyweight mini-workouts ("exercise snacks") that hit the
  big muscle groups and raise the heart rate through the day.
- **Mix** — alternates one stretch session, one workout session, so the neck
  program keeps going while you're losing weight.

Bodyweight only, plus a pull-up bar.

## Run it

```powershell
# one-time build (needs the .NET 8 SDK, already installed)
.\build.cmd

# then run
.\dist\StretchReminder.exe
```

Or during development: `dotnet run --project StretchReminder`.

## Using it

- A small status window opens with a countdown. **Pause/Resume**, **Stretch
  now**, **Exit**, and the reminder interval are right there.
- **Closing or minimizing the window sends it to the tray** — it keeps running.
  Double-click the tray icon to bring it back. The **Exit** button (or tray →
  Exit) quits the app completely.
- When the timer fires, a popup appears bottom-right (with a soft chime, without
  stealing your keyboard focus) showing the next micro-session: each exercise
  has step-by-step instructions and a built-in hold timer.
- **Snooze 5 min** if you're mid-task (Esc also snoozes once you've clicked the
  popup); **Skip** to wait a full interval. Finished sessions advance the
  rotation automatically.
- Tray menu: Open, Stretch now, Pause, Program, Interval, Height & weight,
  Water reminder, **Start with Windows**, Exit.
- The **Program** picker (window or tray) switches between Stretch, Weight
  loss, and Mix. Each program remembers its own place in the rotation.
- **Recommended intervals** are marked in the picker: 45 min for Stretch and
  Mix (frequent + gentle is the point), 60 min for Weight loss (recovery
  between snacks; ~6–8 workouts/day is plenty).
- **Height & weight → BMI**: enter them once (button in the window). The BMI
  and category show in the main window. At BMI ≥ 30 workout popups switch to
  **low-impact mode** — step jacks instead of jumping jacks, fast march
  instead of high knees, incline mountain climbers — to protect knees while
  the weight comes down. Update the weight as it drops; standard versions
  return automatically under BMI 30. (BMI is a crude population measure — it
  can't see muscle — but it's good enough to pick impact levels.)
- **Water reminder**: a quiet tray notification (not a popup) suggesting a
  glass (~250 ml) every hour — roughly 2–2.5 L across a waking day, the
  standard adult guidance; err higher in hot weather or after workouts.
  Toggle it in the window or tray. If you have kidney or heart conditions
  that limit fluids, follow your doctor's target instead.
- **Theme**: tray menu → Theme → System default / Light / Dark. "System
  default" follows the Windows app theme (checked at launch and on switch).
- Settings and rotation state live in `%APPDATA%\StretchReminder\`.

## The Stretch program (rotates through the day)

| # | Session | What it does |
|---|---------|--------------|
| 1 | Neck Wake-Up | Gentle rotations, side tilts, shoulder rolls |
| 2 | Right-Side Release | Upper-trap and levator scapulae stretches (right-biased) |
| 3 | Behind-the-Ear Relief | SCM stretch + suboccipital nods/release — the muscles that refer pain behind the ear |
| 4 | Deep Neck Strength | Chin tucks + 4-direction isometrics — strength makes relief last |
| 5 | Hang & Decompress | Pull-up bar: dead hangs + scapular pulls |
| 6 | Posture Reset | Wall angels, doorway chest stretch, scapular squeezes |

At the default 45-minute interval you'll cycle the whole program about 1½
times in a workday. Consistency beats intensity — expect noticeable change
over 1–2 weeks, not 1–2 days.

## The Weight-loss program

| # | Session | What it does |
|---|---------|--------------|
| 1 | Squats & Jacks | Bodyweight squats, jumping jacks, reverse lunges |
| 2 | Push & Plank | Push-ups, plank, mountain climbers |
| 3 | Bar Work | Pull-ups or slow negatives, hanging knee raises, scapular pulls |
| 4 | Fast Feet | High knees, fast air squats, no-jump burpees |
| 5 | Legs & Glutes | Split squats, glute bridges, wall sit |
| 6 | Full-Body Finisher | Squats, push-ups, jumping jacks |

Every session has a quiet/low-impact option and a scale-down (desk push-ups,
step jacks, marching). Honest framing: these snacks add maybe 150–300 kcal of
burn a day and — more importantly — break up sitting, keep muscle while you
diet, and build the habit. **Weight loss itself is won mostly in the kitchen**
(a modest calorie deficit, plenty of protein); use the popups as the exercise
side of that, not a substitute.

To tweak exercises in either program, edit `StretchReminder/Exercises.cs` and
rebuild.

## A word of caution (please read once)

This is general wellness content, not medical advice.

- Stretch to a **mild pull, never sharp pain**. Don't crank on your head.
- **Stop immediately** if you feel dizziness or light-headedness (especially
  during the SCM stretch), or numbness/tingling running down an arm — and get
  those checked.
- If the spot behind your ear is **swollen, warm, or feels like a tender
  lump**, that may be a lymph node or ear issue rather than a muscle — see a
  doctor instead of stretching it.
- If pain persists beyond ~2 weeks of consistent daily work, worsens, or comes
  with headaches, fever, or arm weakness, see a doctor or physiotherapist.
