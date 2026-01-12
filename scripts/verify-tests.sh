#!/bin/bash

# Script to help Rider recognize new test projects
# Run this if Rider doesn't show your new test projects

echo "🔍 Verifying test project structure..."
echo ""

# Check if test files exist
echo "📁 Test Repository Files:"
find tests/Unit -name "*RepositoryTests.cs" -type f | sort

echo ""
echo "📦 Test Projects in Solution:"
dotnet sln list | grep -i "infrastructure.tests" | sort

echo ""
echo "🔨 Rebuilding solution to refresh Rider cache..."
dotnet build --no-incremental

echo ""
echo "✅ Done! Now try these steps in Rider:"
echo "   1. File → Reload All Projects"
echo "   2. Or: File → Invalidate Caches → Invalidate and Restart"
echo ""
echo "📊 Test Summary:"
echo "   - Total test files: $(find tests/Unit -name "*Tests.cs" -type f | wc -l | xargs)"
echo "   - Repository tests: $(find tests/Unit -name "*RepositoryTests.cs" -type f | wc -l | xargs)"
echo "   - Infrastructure projects: $(dotnet sln list | grep -i 'infrastructure.tests' | wc -l | xargs)"
