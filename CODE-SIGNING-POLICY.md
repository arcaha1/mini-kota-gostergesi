# Code signing policy

Status: preparation only. Not applied, accepted or signed by SignPath Foundation.
Only reviewed binaries built from this project's source in a verifiable CI
process may be signed. No arbitrary uploads or third-party binaries may be signed.
Each release requires manual approval by an authorized human.

Copyright holder: arcah. This is not a verified SignPath or repository identity.
Proposed maintainer/reviewer/approver: arcah, subject to the owner's confirmation
and service acceptance. Repository URL, account links and actual permissions
will be added after publication approval. MFA must be confirmed by the user on
both the source repository and SignPath accounts; no credential belongs here.

[Privacy policy](PRIVACY.md). Removal instructions are in [README](README.md).

After acceptance and signing service activation, the required attribution will
be added to home and release pages:
“Free code signing provided by SignPath.io, certificate by SignPath Foundation”.
This sentence describes the required future attribution, not current service.

Artifact restrictions must enforce ProductName “Mini Kota Göstergesi” and
ProductVersion “1.0.2.0” for this release. Future releases must update all version
attributes together. Approved source commit, CI run, SHA256 and signing outcome
must be recorded. Private signing material must stay in the provider's custody.
The existing desktop shortcut may be updated only after trusted signature and
real Windows launch/quota checks succeed.
