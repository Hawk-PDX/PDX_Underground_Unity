# PDX Underground Project Consolidation Plan

## Editor Scripts Consolidation

After comparing duplicated scripts across the project, here is the consolidation plan:

### 1. EnvironmentTextureGenerator.cs

- **Keep**: Assets/Editor/EnvironmentTextureGenerator.cs
  - This version contains more advanced texture generation capabilities
  - Includes better UI with preview functionality
  - Has more detailed pattern generation algorithms

- **Remove**: Assets/Scripts/Editor/EnvironmentTextureGenerator.cs
  - This is a simpler implementation with fewer features

### 2. EnvironmentMaterialGenerator.cs

- **Keep**: Assets/Editor/EnvironmentMaterialGenerator.cs
  - Contains complete material creation workflows
  - Properly integrated with the overall environment setup pipeline

- **Remove**: Assets/Scripts/Editor/EnvironmentMaterialGenerator.cs
  - Redundant implementation

### 3. Cleanup Actions

1. Remove duplicated scripts from Assets/Scripts/Editor
2. Clean up backup files in Assets/Scripts/Environment/Backups
3. Retain any unique functionality from the removed scripts if needed

### 4. Implementation Steps

1. Backup duplicated files before deletion (for safety)
2. Remove duplicate files
3. Update any references in other scripts
4. Test to ensure all functionality works correctly

## Other Project Structure Improvements

1. Organize script namespaces consistently
2. Ensure proper file organization within the Assets folder
3. Remove any other redundant scripts across the project

