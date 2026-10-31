# Quesadilla

Quesadilla is a modern EVE Online companion application developed by **Izakbar**.

Designed for explorers, hunters, industrialists, wormhole enjoyers and fleet pilots alike, Quesadilla combines overlays, intelligence gathering, combat analytics, mining statistics, mapping and situational awareness tools into a single application.

The goal is simple: provide useful information when and where it is needed without requiring pilots to constantly switch between browser windows, spreadsheets and third-party websites.

## Features

### Overlay System

Quesadilla provides an overlay capable of displaying contextual information whilst playing EVE Online.

Features include:

- Character status monitoring
- System information
- Combat information
- Wormhole information
- Skyhook information
- Nearby activity awareness
- Real-time notifications

### Intel & Activity Monitoring

Stay informed about what is happening around you.

- Intel notifications
- zKillboard activity monitoring
- Configurable popup alerts
- Sound notifications

### Mapping & Navigation

Integrated map tools designed to make navigation faster and easier.

- Real-time location tracking
- EVE system information
- Incursion tracking
- Metaliminal storm tracking
- Wormhole data integration
- UniHoles integration
- Nearby system awareness

### Some Wormhole Support

Tools specifically aimed at wormhole residents and explorers.

- Anoikis information
- Nearest Thera routing
- Nearest Turnur routing

### Combat Analytics

Analyse combat performance directly from EVE logs.

- DPS analysis
- Combat tracking
- Historical combat information

### Mining Analytics

Monitor mining operations in real time.

- Units per minute statistics in graphs

### PVE Analytics

Track income generated through gameplay.

- pre-tax ISK tracking
- pre-tax ISK per minute graph

### OBS integration

Designed with content creators in mind.

- OBS-friendly widgets
- zkill widget http://localhost:654321/obs/zkillfeed
- latest alert http://localhost:654321/obs/latestAlert or http://localhost:654321/obs/latestAlertImage
- Mining per Hour details http://localhost:654321/obs/miningPerHour http://localhost:654321/obs/miningPerHourTable
- Isk Per Hour http://localhost:654321/obs/iskPerHour
- Various windows/parts can be captured by normal OBS capture
- Character locations can be output to seperate files/one file for controlling OBS based on location (useful if abyssal running for example to switch views when you return)

### Multi-Client Support

Built for modern EVE gameplay.

- Multiple client monitoring
- EVE-O Preview integration using a recent version from https://github.com/Proopai/eve-o-preview/releases

## Static Data Export (SDE)

Quesadilla uses data derived from CCP's Static Data Export (SDE).

The source SDE files are **not included** within this repository because several files exceed GitHub's maximum file size limits and would dramatically increase repository size.

The latest SDE can be downloaded directly from CCP:

- https://developers.eveonline.com/docs/data/sde/
- https://developers.eveonline.com/

Download and extract the JSON SDE files into:

```text
GenerateSolarSystemData\SDE
```

The `GenerateSolarSystemData` utility can then be used to generate the data files consumed by Quesadilla.

## Building

Requirements:

- Windows
- Visual Studio 2022 or later
- .NET SDK

Build steps:

```text
1. Clone repository
2. Open solution
3. Restore packages
4. Build
5. Run
```

## Acknowledgements

Quesadilla would not exist without the support, testing and feedback of the EVE Online community.

### Testers

Special thanks to:

- Paul "Hipster" Malcolmson
- Hannah Pares
- Ultimate-pilot Destiny
- Nathalia Itiero
- Zubenelgenubi

### Special Thanks

Special thanks to:

- Kleo Azu for countless discussions, feedback, testing and idea validation.
- Nathalia Itiero for testing, feedback and helping improve the application.

### Other Projects & Libraries

Thanks to:

- EVE-O Preview for inspiration around client detection and multi-client workflows.
- EVE Combat Log Analyzer for helpful reference material during combat analytics development.
- EVEStandard for the ESI library.
- SMT for being an incredibly useful tool.
- Cyotek.Windows.Forms.ColorPicker.
- Atkinson Hyperlegible font for accessibility and readability improvements.

### CCP Games

Quesadilla exists because of EVE Online and its incredible community.

Thanks to CCP Games, Fenris Creations and the EVE Online GM team for their support and for creating a universe worth building tools for.

## Disclaimer

Quesadilla is an independent third-party application.

Quesadilla is not affiliated with, endorsed by, or sponsored by CCP Games.

EVE Online®, EVE®, the EVE logo and all related trademarks are the property of CCP Games.