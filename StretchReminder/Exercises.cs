namespace StretchReminder;

/// <summary>
/// Two rotating programs sharing the same popup mechanics.
/// Stretch: right-side neck stiffness with pain behind the right ear — upper
/// trapezius, levator scapulae, SCM and suboccipitals are the usual culprits —
/// plus deep-neck and scapular strengthening so relief lasts.
/// Weight loss: short bodyweight "exercise snacks" that raise the heart rate
/// and work the big muscle groups. Bodyweight only, plus a pull-up bar.
/// </summary>
public static class Exercises
{
    public const string SafetyFooter =
        "Gentle only — a mild pull is good, sharp pain is not. " +
        "Stop if you feel dizzy or pain/tingling running down the arm.";

    public const string WorkoutFooter =
        "Quality first — keep a pace where you could still talk. Stop if anything " +
        "hurts sharply or you feel dizzy. Sore from yesterday? Do the easy option, " +
        "halve the reps, or skip — rest is part of the plan.";

    // Shared low-impact variant (must be declared before the session arrays that use it).
    private static readonly Exercise StepJacks = new(
        "Step Jacks",
        "full body — cardio, no impact",
        "45 s at a steady pace",
        new[]
        {
            "Step one foot out to the side as the arms sweep overhead.",
            "Back to center, then the other side — keep a brisk rhythm.",
            "No jumping needed; the pace is the workout.",
        },
        45);

    public static readonly Session[] StretchSessions =
    {
        new Session(
            "Neck Wake-Up",
            "Gentle range of motion to warm things up.",
            new[]
            {
                new Exercise(
                    "Slow Look-Overs",
                    "whole neck — rotation",
                    "5 slow turns per side",
                    new[]
                    {
                        "Sit or stand tall, shoulders relaxed.",
                        "Slowly turn your head to the right until you feel a gentle pull.",
                        "Pause 2 seconds, return to center, then turn left.",
                        "Slow and smooth — no forcing, no bouncing.",
                    },
                    60),
                new Exercise(
                    "Ear-to-Shoulder Tilts",
                    "sides of the neck (upper traps)",
                    "5 tilts per side, 2 s pause each",
                    new[]
                    {
                        "Keep both shoulders down and level.",
                        "Tip your right ear toward your right shoulder; pause 2 s.",
                        "Return to center, then tip to the left.",
                        "Only go as far as feels comfortable.",
                    },
                    60),
                new Exercise(
                    "Shoulder Rolls",
                    "upper traps, shoulder girdle",
                    "10 big, slow rolls backward",
                    new[]
                    {
                        "Shrug your shoulders up toward your ears.",
                        "Roll them back and down in a big, slow circle.",
                        "Exhale as they drop; keep the neck loose.",
                    },
                    30),
            }),

        new Session(
            "Right-Side Release",
            "Longer holds for the tight right side.",
            new[]
            {
                new Exercise(
                    "Upper Trapezius Stretch (right)",
                    "right upper trapezius — neck to shoulder",
                    "2 × 30 s right side, then 1 × 30 s left",
                    new[]
                    {
                        "Sit tall; grip the chair edge with your right hand to anchor that shoulder.",
                        "Tip your left ear toward your left shoulder.",
                        "Rest your left hand on your head — let just its weight add a gentle pull.",
                        "Feel it from the right side of the neck into the shoulder. Breathe slowly.",
                        "For the left-side set, mirror everything: anchor the left hand, tip right ear to right shoulder.",
                    },
                    30,
                    "Hand weight only — never pull the head hard."),
                new Exercise(
                    "Levator Scapulae Stretch (right)",
                    "right levator scapulae — neck to shoulder blade",
                    "2 × 30 s",
                    new[]
                    {
                        "Sit tall; anchor your right hand on the chair edge behind you.",
                        "Turn your head about 45° to the left.",
                        "Now look down, nose toward your left armpit.",
                        "Left hand rests on the back of your head for gentle extra weight.",
                        "You should feel it at the back-right of the neck.",
                    },
                    30),
            }),

        new Session(
            "Behind-the-Ear Relief",
            "The muscles that refer pain to behind the ear.",
            new[]
            {
                new Exercise(
                    "SCM Stretch (right)",
                    "right sternocleidomastoid — front of neck, refers behind the ear",
                    "2 × 25 s",
                    new[]
                    {
                        "Sit tall; press your left hand lightly on your right collarbone.",
                        "Tip your left ear toward your left shoulder.",
                        "Gently rotate your face up and to the right — look toward the ceiling corner.",
                        "Feel a stretch along the front-right of the neck. Breathe slowly.",
                    },
                    25,
                    "Stop at once if you feel dizzy or light-headed."),
                new Exercise(
                    "Suboccipital Nods + Release",
                    "small muscles at the base of the skull",
                    "10 tiny nods, then ~30 s fingertip release",
                    new[]
                    {
                        "Tuck your chin slightly, then nod \"yes\" in a tiny range — 10 slow reps.",
                        "Then place fingertips in the groove at the base of your skull, behind the ears.",
                        "Press gently upward into the tender spots and hold, breathing slowly.",
                        "Tender is fine; sharp pain is not.",
                    },
                    45),
            }),

        new Session(
            "Deep Neck Strength",
            "Strength is what makes the relief last.",
            new[]
            {
                new Exercise(
                    "Chin Tucks",
                    "deep neck flexors — the posture muscles",
                    "10 reps, 3 s hold each",
                    new[]
                    {
                        "Sit or stand tall, eyes level.",
                        "Glide your chin straight back — make a double chin. Don't tip the head down.",
                        "Hold 3 s; feel the back of the neck lengthen. Release.",
                    },
                    45),
                new Exercise(
                    "Isometric Presses (4 directions)",
                    "neck stabilizers, all around",
                    "10 s per direction: forehead, back, right side, left side (run the timer once per direction)",
                    new[]
                    {
                        "Place your palm on your forehead; press your head into it — the head must not move.",
                        "About 30–50% effort. Hold 10 s, breathing normally.",
                        "Repeat with the hand on the back of the head, then the right side, then the left.",
                    },
                    10,
                    "Build up gently — start easy, never press to pain."),
            }),

        new Session(
            "Hang & Decompress",
            "Use the pull-up bar to unload the neck and shoulders.",
            new[]
            {
                new Exercise(
                    "Dead Hang",
                    "spinal decompression, lats, shoulders",
                    "2 × 20–30 s, rest ~30 s between",
                    new[]
                    {
                        "Grip the bar about shoulder-width; step off slowly.",
                        "Let your shoulders shrug up by your ears; relax the neck and just hang.",
                        "Breathe slowly; feel the spine lengthen.",
                        "Step down before your grip gives out.",
                    },
                    30,
                    "Keep your feet close to the floor or a box."),
                new Exercise(
                    "Scapular Pulls",
                    "lower traps and shoulder-blade control — posture support",
                    "6–8 reps, 2 s hold each",
                    new[]
                    {
                        "From a dead hang, keep your elbows completely straight.",
                        "Pull your shoulder blades down and together — your chest lifts slightly.",
                        "Hold 2 s, then relax back into the full hang.",
                    },
                    40),
            }),

        new Session(
            "Posture Reset",
            "Undo the desk: chest, upper back, shoulder blades.",
            new[]
            {
                new Exercise(
                    "Wall Angels",
                    "thoracic mobility + scapular control",
                    "8 slow reps",
                    new[]
                    {
                        "Stand with your butt, upper back, and head against a wall.",
                        "Keep the chin level or gently tucked — if the head won't reach the wall comfortably, let it hover.",
                        "Arms in a \"W\", backs of the hands toward the wall.",
                        "Slide the arms up toward a \"Y\" and back down, keeping contact as best you can.",
                        "It's harder than it looks — smooth and slow beats range.",
                    },
                    60,
                    "Never tilt the head back to force it to the wall."),
                new Exercise(
                    "Doorway Chest Stretch",
                    "pecs — a tight chest drags the head forward",
                    "30 s per side",
                    new[]
                    {
                        "Forearm on the door frame, elbow at shoulder height.",
                        "Step through the doorway until you feel a stretch across the chest.",
                        "Keep the neck relaxed. Switch arms.",
                    },
                    30),
                new Exercise(
                    "Scapular Squeezes",
                    "mid-back — rhomboids and mid traps",
                    "10 reps, 5 s hold each",
                    new[]
                    {
                        "Sit tall; squeeze your shoulder blades back and slightly down.",
                        "Imagine tucking them into your back pockets.",
                        "Hold 5 s, relax. Don't let the shoulders shrug up.",
                    },
                    60),
            }),
    };

    public static readonly Session[] WorkoutSessions =
    {
        new Session(
            "Squats & Jacks",
            "Big lower-body movers to spike the heart rate.",
            new[]
            {
                new Exercise(
                    "Bodyweight Squats",
                    "quads, glutes — the biggest calorie burners",
                    "15–20 reps",
                    new[]
                    {
                        "Feet shoulder-width, toes slightly out, chest up.",
                        "Sit the hips back and down until thighs are near parallel.",
                        "Drive up through the heels; knees track over the toes.",
                        "Slow down, strong up.",
                    },
                    60),
                new Exercise(
                    "Jumping Jacks",
                    "full body — cardio",
                    "45 s at a steady pace",
                    new[]
                    {
                        "Soft knees, land quietly, arms all the way overhead.",
                        "Keep a rhythm you can sustain the whole 45 s.",
                        "Quiet option: step one foot out at a time (\"step jacks\").",
                    },
                    45,
                    null,
                    StepJacks),
                new Exercise(
                    "Reverse Lunges",
                    "quads, glutes, balance",
                    "8–10 per leg, alternating",
                    new[]
                    {
                        "Step one foot back and lower until the back knee hovers above the floor.",
                        "Torso tall; front knee over the ankle.",
                        "Push through the front heel to stand, then switch legs.",
                    },
                    60),
            }),

        new Session(
            "Push & Plank",
            "Upper-body push plus core.",
            new[]
            {
                new Exercise(
                    "Push-Ups",
                    "chest, shoulders, triceps, core",
                    "8–15 reps",
                    new[]
                    {
                        "Hands under the shoulders, body one straight line head to heels.",
                        "Lower the chest with elbows about 45° from the body.",
                        "Press up without letting the hips sag.",
                        "Too hard? Hands on a desk or drop to the knees. Too easy? Slow the lowering.",
                    },
                    45),
                new Exercise(
                    "Plank",
                    "whole core",
                    "30–45 s hold",
                    new[]
                    {
                        "Forearms under the shoulders, feet back.",
                        "Squeeze the glutes; one straight line from head to heels.",
                        "Don't let the hips sag or pike up. Keep breathing.",
                    },
                    40),
                new Exercise(
                    "Mountain Climbers",
                    "core + cardio",
                    "30 s steady",
                    new[]
                    {
                        "From a push-up position, drive the knees toward the chest, alternating.",
                        "Hips stay level with the shoulders — don't bounce them up.",
                        "Slow the pace rather than lose the position.",
                    },
                    30,
                    null,
                    new Exercise(
                        "Incline Mountain Climbers",
                        "core + cardio — easier on wrists and knees",
                        "30 s steady",
                        new[]
                        {
                            "Hands on a sturdy desk or chair seat, body in a straight line.",
                            "Drive the knees toward the chest, alternating, hips level.",
                            "Brisk but controlled — slow down rather than bounce.",
                        },
                        30)),
            }),

        new Session(
            "Bar Work",
            "Pull-up bar day — pulling builds the most muscle per rep.",
            new[]
            {
                new Exercise(
                    "Pull-Ups or Slow Negatives",
                    "lats, arms, grip",
                    "3–8 pull-ups, or 2–3 slow negatives",
                    new[]
                    {
                        "Full grip on the bar, hands about shoulder-width.",
                        "Pull the chest toward the bar without swinging.",
                        "Keep the neck long and chin neutral — don't crane toward the bar.",
                        "Can't pull up yet? Step up to the top from a sturdy chair or box and lower over 3–5 s.",
                        "Step down before your grip gives out.",
                    },
                    60,
                    "Negatives tear muscle down the most — 2–3 is plenty. Skip them if elbows or arms are still sore."),
                new Exercise(
                    "Hanging Knee Raises",
                    "abs, hip flexors, grip",
                    "8–12 reps",
                    new[]
                    {
                        "From a dead hang, lift the knees to hip height.",
                        "Neck relaxed, chin neutral — don't jut the chin as you lift.",
                        "Lower with control — no swinging between reps.",
                        "Smaller range is fine; control is the point.",
                    },
                    45),
                new Exercise(
                    "Scapular Pulls",
                    "shoulder-blade control — posture support",
                    "6–8 reps, 2 s hold each",
                    new[]
                    {
                        "From a dead hang, keep the elbows completely straight.",
                        "Pull the shoulder blades down and together — the chest lifts slightly.",
                        "Hold 2 s, then relax back into the hang.",
                    },
                    40),
            }),

        new Session(
            "Fast Feet",
            "A short cardio hit — you should end a bit out of breath.",
            new[]
            {
                new Exercise(
                    "High Knees",
                    "cardio, hip flexors",
                    "40 s",
                    new[]
                    {
                        "Run in place, knees up toward hip height.",
                        "Land softly on the balls of the feet, arms pumping.",
                        "Quiet option: fast march with high knees instead of running.",
                    },
                    40,
                    null,
                    new Exercise(
                        "Fast March",
                        "cardio, hip flexors — no impact",
                        "40 s",
                        new[]
                        {
                            "March in place as fast as you can, knees toward hip height.",
                            "Pump the arms; stay tall, land soft.",
                        },
                        40)),
                new Exercise(
                    "Fast Air Squats",
                    "legs + heart rate",
                    "15–20 quick reps",
                    new[]
                    {
                        "Same form as regular squats — hips back, chest up.",
                        "Brisker tempo, but never at the cost of form.",
                    },
                    40),
                new Exercise(
                    "No-Jump Burpees",
                    "full body",
                    "6–10 reps",
                    new[]
                    {
                        "Squat down, hands to the floor.",
                        "Step the feet back to a plank, one at a time.",
                        "Step back in and stand up tall. That's one.",
                        "Keep breathing — pace it so you could still talk.",
                    },
                    60),
            }),

        new Session(
            "Legs & Glutes",
            "Strength for the biggest muscles — the highest burn.",
            new[]
            {
                new Exercise(
                    "Split Squats",
                    "quads, glutes",
                    "8–10 per leg (all reps one side, then switch)",
                    new[]
                    {
                        "Take a long stance, back heel up.",
                        "Lower straight down until the back knee hovers above the floor.",
                        "Drive up through the front heel; torso stays tall.",
                    },
                    60),
                new Exercise(
                    "Glute Bridges",
                    "glutes, hamstrings",
                    "15–20 reps, 2 s squeeze at the top",
                    new[]
                    {
                        "Lie on your back, knees bent, feet flat near the hips.",
                        "Drive the hips up until shoulders-to-knees is one line.",
                        "Squeeze the glutes hard at the top, lower slowly.",
                    },
                    60),
                new Exercise(
                    "Wall Sit",
                    "quads — isometric finisher",
                    "30–45 s",
                    new[]
                    {
                        "Back flat on the wall, slide down until thighs are near parallel.",
                        "Knees over the ankles; hands off the thighs.",
                        "Breathe — the legs should burn, the knees should not hurt.",
                    },
                    40),
            }),

        new Session(
            "Full-Body Finisher",
            "One round of everything at once.",
            new[]
            {
                new Exercise(
                    "Air Squats",
                    "quads, glutes",
                    "15 reps",
                    new[]
                    {
                        "Feet shoulder-width, hips back and down to near parallel.",
                        "Drive up through the heels, chest up the whole time.",
                    },
                    45),
                new Exercise(
                    "Push-Ups",
                    "chest, shoulders, triceps",
                    "8–12 reps",
                    new[]
                    {
                        "Body one straight line; chest down, elbows about 45°.",
                        "Scale on a desk or knees if form breaks.",
                    },
                    40),
                new Exercise(
                    "Jumping Jacks",
                    "full body — cardio",
                    "45 s to finish",
                    new[]
                    {
                        "Soft knees, land quietly, arms overhead.",
                        "Quiet option: step jacks.",
                        "Done — shake it out and drink some water.",
                    },
                    45,
                    null,
                    StepJacks),
            }),
    };
}
