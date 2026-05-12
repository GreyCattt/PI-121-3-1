#!/bin/bash

# Скрипт для додавання тестових даних через API для перевірки пошуку й фільтрів
# Запусти це після того, як PL/WebAPI запущений на http://localhost:5184

BASE_URL="http://localhost:5184/api"

echo "=== Додавання тестових даних ==="

# Спершу треба додати користувачів через SQL (поки немає POST /users в API)
# Тому я додам лоти напряму. Припускаємо, що користувачі та категорії вже в БД з ID 1

echo ""
echo "1. Додавання лота: iPhone 15 Pro"
curl -X POST "$BASE_URL/lots" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "iPhone 15 Pro",
    "description": "Смартфон Apple iPhone 15 Pro, 256GB, Silver",
    "startingPrice": 1200,
    "startTime": "2026-05-13T10:00:00Z",
    "endTime": "2026-05-20T10:00:00Z",
    "categoryId": 1,
    "sellerId": 1
  }'

echo ""
echo ""
echo "2. Додавання лота: Gold Watch"
curl -X POST "$BASE_URL/lots" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Gold Watch",
    "description": "Елегантний золотий наручний годинник, швейцарський механізм",
    "startingPrice": 500,
    "startTime": "2026-05-13T12:00:00Z",
    "endTime": "2026-05-25T12:00:00Z",
    "categoryId": 1,
    "sellerId": 1
  }'

echo ""
echo ""
echo "3. Додавання лота: Laptop Dell XPS"
curl -X POST "$BASE_URL/lots" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Laptop Dell XPS 15",
    "description": "Потужний ноутбук для роботи та ігор",
    "startingPrice": 1500,
    "startTime": "2026-05-13T14:00:00Z",
    "endTime": "2026-05-22T14:00:00Z",
    "categoryId": 1,
    "sellerId": 1
  }'

echo ""
echo ""
echo "=== Тестування пошуку й фільтрів ==="

echo ""
echo "A. Пошук за словом 'phone' (має знайти iPhone 15 Pro):"
curl -X GET "$BASE_URL/lots/search?searchQuery=phone" \
  -H "accept: application/json"

echo ""
echo ""
echo "B. Пошук за діапазоном цін 300-600 (золотий годинник):"
curl -X GET "$BASE_URL/lots/search?minPrice=300&maxPrice=600" \
  -H "accept: application/json"

echo ""
echo ""
echo "C. Пошук 'gold' (золотий годинник):"
curl -X GET "$BASE_URL/lots/search?searchQuery=gold" \
  -H "accept: application/json"

echo ""
echo ""
echo "=== Готово ==="
