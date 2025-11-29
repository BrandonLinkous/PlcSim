![PLC System Tests](https://github.com/BrandonLinkous/PlcSim/actions/workflows/system-tests.yml/badge.svg)

<br>

# 📡 PLC System Test Automation

This repository contains a mock PLC simulator and a fully automated system testing pipeline representing real industrial automation workflows.
The goal is to validate PLC behavior end-to-end using isolated, reproducible, container-based testing.

<br>

## 🚀 Features

* 🧪 Automated NUnit system tests against a live PLC container

* 🐳 Docker Compose orchestrates PLC + Tests

* ❤️ Health-checked pump control API simulation

* 🔄 CI pipeline enforcing test verification on every commit/PR

* 📦 Artifacts available from GitHub Actions test runs

* 🧭 Relevant to System Test Engineer workflows

<br>

## 🏗 Project Structure
```
root/
├─ PlcSim/         → .NET Web API simulating PLC tank/pump logic
├─ TestHarness/    → Client wrapper for PLC HTTP endpoints
├─ SystemTests/    → NUnit automated system tests
└─ docker/         → Dockerfiles + docker-compose for full automation
```

<br>

## 🧩 PLC Simulator Logic Summary
The following rules simulate real pump safety logic commonly enforced in industrial PLCs:

| Condition / Action | Resulting Behavior |
|------------------|------------------|
| `/start` command received | Pump starts **only if** high-level alarm is **not** latched |
| `/stop` command received | Pump stops immediately |
| Pump running | Tank level increases by **+5% per second** (until full) |
| Level ≥ 90% | High-level alarm triggers **and** pump automatically stops |
| `/reset-alarm` | Alarm latch clears **when** level falls below threshold |
| `/reset` (system reset) | Pump stops, alarm clears, tank drains to **0%** |

<br>

## 🧪 Automated System Tests

Located in: `SystemTests/`

These tests verify end-to-end PLC behavior while running live inside Docker:

| Requirement ID | Verification |
|----------------|-------------|
| R1 | Pump starts on command |
| R2 | Alarm triggers when tank reaches high level |
| R3 | Pump automatically stops when alarm is active |
| R4 | Manual stop command stops pump |
| R5 | Tank level increases while pump is running |
| R6 | Pump cannot restart until alarm is reset **and** tank level is safe |

These run inside Docker and simulate real PLC network communication.

<br>

## 🐳 Running Locally (without CI)

From root of repo:
```
cd docker
docker compose up --build --abort-on-container-exit
```

✔ Spins up PLC container  
✔ Waits for health check  
✔ Runs system tests in separate test container  
✔ Shuts down on completion

🎉 Same environment as CI

<br>

## 🧰 Developer Usage

Rebuild PLC simulator only:
```
docker compose build plc-sim --no-cache
```

View logs:
```
docker logs docker-plc-sim-1
```

Run tests directly:
```
dotnet test SystemTests/SystemTests.csproj
```

<br>

## 🤖 Continuous Integration

GitHub Actions workflow:
`.github/workflows/system-tests.yml`

🔹 Every commit and pull request triggers:

1. Build PLC container

2. Start PLC + wait for health

3. Build and run test container

4. Publish .trx test report artifacts

5. Set pass/fail status in PR

No merge allowed unless all system tests pass 🚦
