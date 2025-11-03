# PC Builder Module

This module implements a Configure-Price-Quote (CPQ) system for custom PC builds.

## Responsibilities

- Component catalog management (CPU, GPU, RAM, etc.)
- Compatibility validation using rules engine
- Price calculation and quoting
- Configuration saving and sharing
- Build validation

## Planned Implementation

This module will use **Microsoft RulesEngine** (JSON-based rules) to manage compatibility logic externally from C# code.

### Key Features (To Be Implemented)

1. **Component Compatibility Rules**
   - CPU socket matching with motherboard
   - RAM type and speed compatibility
   - Power supply wattage requirements
   - Case size and motherboard form factor
   - Cooling solution compatibility

2. **Pricing Engine**
   - Component pricing
   - Labor cost calculation
   - Markup and discount rules
   - Quote generation

3. **Configuration Management**
   - Save and load configurations
   - Share configurations with customers
   - Configuration history

## Technology

- **Microsoft RulesEngine** - Lightweight, JSON-based rules evaluation
- Decouples business rules from application code
- Easy to update rules without recompilation
