# Spooky Mansion III

A first person stealth horror game in a mansion.

- Play: [itch.io](https://unitedfailures.itch.io/spooky-mansion-iii)
- Made: June to July 2023 for Mini Jam 135: Deception
- Team: [@mfclinton](https://github.com/mfclinton) (programming), [@WoodrowCrawford](https://github.com/WoodrowCrawford) (programming), [Jose-lese](https://jose-lese.itch.io) (art), [Pheonyx Games](https://pheonyxgames.itch.io) (art), [erasound](https://erasound.itch.io) (audio)
- Engine: Unity, C#

This is an export of a private repo with only the code we wrote. Art, audio, the Unity project files, and third party plugins aren't included. The history is squashed into one commit.

## What I built

- Enemies can hear you. Every footstep sends out noise in a radius that shrinks when you crouch and grows when you sprint, jump, or shoot, and any enemy inside it goes to check out where it came from.
- Enemies see with a vision cone and a line of sight check. They wander the mansion on a NavMesh, chase you once they spot you, and keep going for a few seconds after they lose you.
- Lights flicker at random, and the glowing bulbs dim right along with them. Every so often the whole mansion blacks out, your lantern included, and the enemies get moved somewhere new in the dark.
- I hooked up FMOD across the game, with sounds for enemies spotting you, losing you, and hearing a noise, plus footsteps, reloads, and doors. The audio ambience shifts depending on whether the enemies are wandering, investigating a noise, or chasing you.
- I made an editor tool that swaps imported level pieces, like the door frames, for their prefabs all at once. The shelves also fill themselves with random props when the level loads.
- Doors, cabinets, desks, keys, and hidden walls you can interact with, and guns with aiming, recoil, and reloading.
