---
title: Articles
---

# Articles
Welcome to the GoRogue articles!  These articles are designed act as conceptual documentation and helpful how-to articles that can help get you started using GoRogue.  [Getting started](~/articles/getting-started.md) instructions are provided, as well as how-to articles that outline various GoRogue features and some usage examples.  While these articles do not comprehensively cover every aspect of a given feature (the [API Documentation](~/api/index.md) is helpful in this regard), the pages are generally meant to provide you with a basic understanding of the feature they cover, and some examples that will help you get started using it.

# Upgrading from 2.x to 3.0
If you are a GoRogue 2.x user looking to upgrade to 3.0, a wiki page detailing the changes and upgrade process can be found [here](~/articles/2-to-3-upgrade-guide.md).

# Upgrading from 1.x to 2.0
If you are a GoRogue 1.x user looking to upgrade to 2.0, a wiki page detailing the changes and upgrade process can be found [here](~/articles/1-to-2-upgrade-guide.md).

# Library Design Concepts
Generally, GoRogue is designed to provide a set of tools that are minimally intrusive upon your own architecture.  More specifically, there are two basic categories of GoRogue features -- "core" features, and "game framework" features.

## Core Features
The majority of the library falls under the core category.  This category is composed of the root `GoRogue` namespace and all sub-namespaces except `GoRogue.GameFramework`.  These core features are designed to provide generic data structure and algorithm implementations that may assist you in creating your game.  These features, by design, work without asserting much of anything about what your game is or how it works, or what your code architecture looks like.  While some data structures, like `ISpatialMap` implementations, can be used to hold data related to a map or game, they purposefully avoid relying on concrete data structures for maps, objects on maps, or even game data.  This ensures that these features can be used in the widest possible array of use cases, without the library needing to "specify" what your game architecture must be and how your data is stored.  These core features still have how-to articles that are designed to help you understand how they work, as well as complete API documentation.

## Game Framework Features
The second category of features is the game framework category. These features are all contained within the `GoRogue.GameFramework` namespace. Unlike the core features, their purpose is to combine GoRogue core features into a coherent, concrete structure that can be used as a framework for your game, and build upon those features to create functionality that may apply to many use cases.
