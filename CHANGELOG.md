# Changelog

## 0.0.1

First release. It is possible to connect to the Integra board with ETHM-1 interface to read and write its state.

- NtegraTcpClient for low-level communication
  - Includes checksum and response validation
- NtegraController abstracting several commands:
  - getting inputs (zones) state
  - getting outputs state
  - setting outputs state
  - getting temperature
  - getting system metadata
