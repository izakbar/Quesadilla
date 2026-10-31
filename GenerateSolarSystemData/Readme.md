# Static Data Export (SDE) Files

The files required by `GenerateSolarSystemData` are not included in this repository.

## Why are the SDE files excluded?

The EVE Online Static Data Export (SDE) dataset contains several very large JSONL files. Some of these files exceed GitHub's maximum file size limit of 100 MB and therefore cannot be stored in a standard Git repository hosted on GitHub.

Examples include:

| File | Approximate Size |
|--------|--------:|
| mapMoons.jsonl | 200+ MB |
| types.jsonl | 140+ MB |
| missions.jsonl | 50+ MB |

To keep the repository lightweight, cloneable, and compatible with GitHub, the SDE data files are intentionally excluded from source control.

## Downloading the SDE

The SDE can be obtained directly from CCP Games.

Current documentation:

- [EVE Online Developers - Static Data Export](https://developers.eveonline.com/docs/data/sde/)
- [EVE Online Developers Portal](https://developers.eveonline.com/)

Download the latest JSON-formatted SDE package from CCP and extract the contents into:

```text
GenerateSolarSystemData/SDE/
```

After extraction, the directory should contain files such as:

```text
GenerateSolarSystemData/
└─ SDE/
   ├─ types.jsonl
   ├─ mapMoons.jsonl
   ├─ missions.jsonl
   └─ ...
```

## Generating Quesadilla Data

Once the SDE files have been downloaded and extracted:

1. Open the solution.
2. Build the `GenerateSolarSystemData` project.
3. Run the application.
4. The tool will process the CCP SDE and generate the data files required by Quesadilla. You need to copy to the Quesadilla project directory (or local directory if for your locally running version)

## Updating to a New SDE Version

When CCP releases a new SDE:

1. Download the latest SDE package.
2. Replace the contents of `GenerateSolarSystemData/SDE/`.
3. Re-run `GenerateSolarSystemData`.
4. You need to copy to the Quesadilla project directory (or local directory if for your locally running version).
5. Commit the regenerated Quesadilla data files if required, 

## Attribution

The EVE Online Static Data Export is provided by CCP Games and remains the property of CCP Games.

EVE Online and the EVE logo are the registered trademarks of CCP Games.