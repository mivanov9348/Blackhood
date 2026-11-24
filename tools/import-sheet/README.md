# Character sheet → JSON

Converts the character-sheet document (`Devils_Seat_NPC_Profili.docx`) into
`content/characters/<CODE>.json`, merged with the authored additions in `additions.mjs`.

You only need this when the **document** changes. Day-to-day editing happens in the
admin editor (`/admin`), which owns the database. Use its "Export to JSON" button to
write files back.

## 1. Extract the text from the .docx (PowerShell)

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$z = [System.IO.Compression.ZipFile]::OpenRead("C:\path\to\Devils_Seat_NPC_Profili.docx")
$r = New-Object System.IO.StreamReader($z.GetEntry("word/document.xml").Open())
$x = $r.ReadToEnd(); $r.Close(); $z.Dispose()
$x = $x -replace '</w:p>', "`n" -replace '<[^>]+>', ''
[System.Net.WebUtility]::HtmlDecode($x) | Set-Content sheet.txt -Encoding utf8
```

## 2. Generate the JSON files

```powershell
node tools/import-sheet/parse-sheet.mjs sheet.txt          # writes content/characters/*.json
node tools/import-sheet/parse-sheet.mjs sheet.txt --dry    # prints the first one only
```

The script warns about label-like lines it does not recognise (new fields in the template).

## 3. Load into the database

The API imports only characters **missing** from the database at startup. To reload a
character that already exists, delete it in the admin editor (or the database) and
restart the API.
