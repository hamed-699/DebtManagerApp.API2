# المرحلة الأولى: بناء التطبيق
# نستخدم صورة الدوت نت اس دي كي (SDK) الرسمية كقاعدة
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملف المشروع واستعادة الحزم (هذا يسرع عملية البناء عند تغيير الكود فقط)
COPY ["DebtManagerApp.API.csproj", "."]
RUN dotnet restore "./DebtManagerApp.API.csproj"

# نسخ باقي ملفات المشروع وبناء التطبيق
COPY . .
WORKDIR "/src/."
RUN dotnet publish "DebtManagerApp.API.csproj" -c Release -o /app/publish

# المرحلة الثانية: تشغيل التطبيق
# نستخدم صورة الدوت نت رنتايم (Runtime) الأخف وزناً
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render (والمنصات المشابهة) تتوقع أن التطبيق يستمع على منفذ محدد في متغير البيئة PORT
# الـ Dockerfile سيعرف متغير البيئة هذا، و الكود الخاص بنا (Program.cs) سيقرأه
ENV PORT 8080
EXPOSE 8080

# نقطة الدخول لتشغيل الخادم
ENTRYPOINT ["dotnet", "DebtManagerApp.API.dll"]
