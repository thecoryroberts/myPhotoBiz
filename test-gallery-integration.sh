#!/bin/bash

# Gallery Integration Test Script
# Run this to verify the gallery functionality end-to-end

echo "🔍 Gallery Functionality Integration Test"
echo "=========================================="
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
PASSED=0
FAILED=0

# Helper function to print test results
test_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ PASSED${NC}: $2"
        ((PASSED++))
    else
        echo -e "${RED}✗ FAILED${NC}: $2"
        ((FAILED++))
    fi
}

# Check 1: Controllers exist
echo -e "\n${YELLOW}[1] Checking Controllers...${NC}"
if [ -f "Controllers/GalleriesController.cs" ]; then
    test_result 0 "GalleriesController.cs found"
else
    test_result 1 "GalleriesController.cs NOT found"
fi

if [ -f "Controllers/GalleryController.cs" ]; then
    test_result 0 "GalleryController.cs found"
else
    test_result 1 "GalleryController.cs NOT found"
fi

if [ -f "Controllers/PhotosController.cs" ]; then
    test_result 0 "PhotosController.cs found"
else
    test_result 1 "PhotosController.cs NOT found"
fi

# Check 2: Models exist
echo -e "\n${YELLOW}[2] Checking Models...${NC}"
if [ -f "Models/Gallery.cs" ]; then
    test_result 0 "Gallery.cs found"
else
    test_result 1 "Gallery.cs NOT found"
fi

if [ -f "Models/GallerySession.cs" ]; then
    test_result 0 "GallerySession.cs found"
else
    test_result 1 "GallerySession.cs NOT found"
fi

if [ -f "Models/GalleryAccess.cs" ]; then
    test_result 0 "GalleryAccess.cs found"
else
    test_result 1 "GalleryAccess.cs NOT found"
fi

if [ -f "Models/Album.cs" ]; then
    test_result 0 "Album.cs found"
else
    test_result 1 "Album.cs NOT found"
fi

if [ -f "Models/Photo.cs" ]; then
    test_result 0 "Photo.cs found"
else
    test_result 1 "Photo.cs NOT found"
fi

# Check 3: Services exist
echo -e "\n${YELLOW}[3] Checking Services...${NC}"
if [ -f "Services/IGalleryService.cs" ] && [ -f "Services/GalleryService.cs" ]; then
    test_result 0 "GalleryService interface and implementation found"
else
    test_result 1 "GalleryService files NOT found"
fi

if [ -f "Services/IImageService.cs" ] && [ -f "Services/ImageService.cs" ]; then
    test_result 0 "ImageService interface and implementation found"
else
    test_result 1 "ImageService files NOT found"
fi

if [ -f "Services/IAlbumService.cs" ] && [ -f "Services/AlbumService.cs" ]; then
    test_result 0 "AlbumService interface and implementation found"
else
    test_result 1 "AlbumService files NOT found"
fi

# Check 4: Views exist
echo -e "\n${YELLOW}[4] Checking Views...${NC}"
if [ -f "Views/Galleries/Index.cshtml" ]; then
    test_result 0 "Galleries Index view found"
else
    test_result 1 "Galleries Index view NOT found"
fi

if [ -f "Views/Gallery/Index.cshtml" ]; then
    test_result 0 "Gallery client view found"
else
    test_result 1 "Gallery client view NOT found"
fi

if [ -d "Views/Galleries" ] && [ -f "Views/Galleries/_CreateGalleryModal.cshtml" ]; then
    test_result 0 "Gallery modals found"
else
    test_result 1 "Gallery modals NOT found"
fi

# Check 5: Database configuration
echo -e "\n${YELLOW}[5] Checking Database Configuration...${NC}"
if grep -q "DbSet<Gallery>" Data/ApplicationDbContext.cs; then
    test_result 0 "Gallery DbSet configured in ApplicationDbContext"
else
    test_result 1 "Gallery DbSet NOT configured"
fi

if grep -q "DbSet<GallerySession>" Data/ApplicationDbContext.cs; then
    test_result 0 "GallerySession DbSet configured"
else
    test_result 1 "GallerySession DbSet NOT configured"
fi

if grep -q "DbSet<GalleryAccess>" Data/ApplicationDbContext.cs; then
    test_result 0 "GalleryAccess DbSet configured"
else
    test_result 1 "GalleryAccess DbSet NOT configured"
fi

if grep -q "ConfigureGalleryRelationships" Data/ApplicationDbContext.cs; then
    test_result 0 "Gallery relationships configured"
else
    test_result 1 "Gallery relationships NOT configured"
fi

# Check 6: JavaScript handlers
echo -e "\n${YELLOW}[6] Checking JavaScript Handlers...${NC}"
if [ -f "wwwroot/js/pages/galleries.js" ]; then
    test_result 0 "galleries.js found"
    
    if grep -q "function showCreateGalleryModal" wwwroot/js/pages/galleries.js; then
        test_result 0 "showCreateGalleryModal function defined"
    else
        test_result 1 "showCreateGalleryModal function NOT defined"
    fi
    
    if grep -q "function showGalleryDetails" wwwroot/js/pages/galleries.js; then
        test_result 0 "showGalleryDetails function defined"
    else
        test_result 1 "showGalleryDetails function NOT defined"
    fi
else
    test_result 1 "galleries.js NOT found"
fi

# Check 7: Build compilation
echo -e "\n${YELLOW}[7] Checking Build Compilation...${NC}"
dotnet build 2>&1 | grep -i "error" > /dev/null
if [ $? -ne 0 ]; then
    test_result 0 "Project builds without errors"
else
    test_result 1 "Project has compilation errors"
fi

# Check 8: Program.cs registration
echo -e "\n${YELLOW}[8] Checking Service Registration...${NC}"
if grep -q "IGalleryService.*GalleryService" Program.cs; then
    test_result 0 "IGalleryService registered in Program.cs"
else
    test_result 1 "IGalleryService NOT registered"
fi

if grep -q "IImageService.*ImageService" Program.cs; then
    test_result 0 "IImageService registered in Program.cs"
else
    test_result 1 "IImageService NOT registered"
fi

# Summary
echo ""
echo "=========================================="
echo -e "${GREEN}✓ PASSED: $PASSED${NC} | ${RED}✗ FAILED: $FAILED${NC}"
echo "=========================================="

if [ $FAILED -eq 0 ]; then
    echo -e "\n${GREEN}🎉 All gallery functionality checks passed!${NC}"
    exit 0
else
    echo -e "\n${RED}⚠️  Some checks failed. Please review the output above.${NC}"
    exit 1
fi
