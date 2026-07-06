.PHONY: help setup up up-d down down-v build logs-api logs-db rebuild-api \
        test test-verbose test-build test-dal test-bll test-api test-local

DOCKER_COMPOSE := docker compose
DOTNET_TEST    := dotnet test src/EMI.EmployeeManagement.sln

.DEFAULT_GOAL := help

help: ## Show available commands
	@echo "EMI Employee Management"
	@echo ""
	@echo "Project:"
	@grep -E '^[a-zA-Z0-9_-]+:.*## ' $(MAKEFILE_LIST) | grep -v '^test' | awk 'BEGIN {FS = ":.*## "}; {printf "  \033[36m%-18s\033[0m %s\n", $$1, $$2}'
	@echo ""
	@echo "Unit tests (Docker):"
	@grep -E '^test[a-zA-Z0-9_-]*:.*## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*## "}; {printf "  \033[36m%-18s\033[0m %s\n", $$1, $$2}'

# ── Project ──────────────────────────────────────────────────────────────────

setup: ## Copy .env.example to .env if it does not exist
	@test -f .env || cp .env.example .env
	@echo ".env ready"

up: ## Build and start API + PostgreSQL (foreground)
	$(DOCKER_COMPOSE) up --build

up-d: ## Build and start API + PostgreSQL (background)
	$(DOCKER_COMPOSE) up -d --build

down: ## Stop API and PostgreSQL
	$(DOCKER_COMPOSE) down

down-v: ## Stop services and remove PostgreSQL data volume
	$(DOCKER_COMPOSE) down -v

build: ## Build API and database images
	$(DOCKER_COMPOSE) build

logs-api: ## Follow API container logs
	$(DOCKER_COMPOSE) logs -f api

logs-db: ## Follow database container logs
	$(DOCKER_COMPOSE) logs -f db

rebuild-api: ## Rebuild and restart API after code changes
	$(DOCKER_COMPOSE) up --build api

# ── Unit tests ───────────────────────────────────────────────────────────────

test: ## Run all unit tests in Docker (no local .NET SDK required)
	$(DOCKER_COMPOSE) --profile test run --rm --build tests

test-verbose: ## Run unit tests in Docker with verbose output
	$(DOCKER_COMPOSE) --profile test run --rm tests -- --verbosity normal

test-build: ## Build the test Docker image without running tests
	$(DOCKER_COMPOSE) --profile test build tests

test-dal: ## Run DAL unit tests in Docker
	$(DOCKER_COMPOSE) --profile test run --rm tests -- --filter "FullyQualifiedName~DAL"

test-bll: ## Run BLL unit tests in Docker
	$(DOCKER_COMPOSE) --profile test run --rm tests -- --filter "FullyQualifiedName~BLL"

test-api: ## Run API controller unit tests in Docker
	$(DOCKER_COMPOSE) --profile test run --rm tests -- --filter "FullyQualifiedName~API"

test-local: ## Run all unit tests locally (requires .NET 8 SDK)
	$(DOTNET_TEST)
