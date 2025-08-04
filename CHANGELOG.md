# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

_nothing changed yet_


## [v0.2.0] - 2025-08-04

### Added

- Explicit nullable
- Strong assembly name signing

### Changed

- The DNS record type is now strong typed via enum `DnsRecordType`
- The response status is now strong typed via enum `ResponseStatus`

### Removed

- Unneccessary internal constructor (for UnitTests)
- `Models` sub-namespace


## [v0.1.0] - 2024-06-21

_initial release, DNS only_

### Added

- RegEx pattern for DomainName, ClientRequestId and RecordType
- Basic client to initialize the API
- API actions to read and update DNS entries of a domain



[Unreleased]: https://github.com/AM-WD/netcup-ccp-api/compare/v0.2.0...HEAD

[v0.2.0]: https://github.com/AM-WD/netcup-ccp-api/compare/v0.1.0...v0.2.0
[v0.1.0]: https://github.com/AM-WD/netcup-ccp-api/commits/v0.1.0
