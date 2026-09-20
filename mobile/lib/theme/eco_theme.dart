import 'package:flutter/material.dart';

class EcoTheme {
  static const Color primary = Color(0xFF1F5E4A);
  static const Color primaryDark = Color(0xFF163F33);
  static const Color backgroundMain = Color(0xFFF7F9F7);
  static const Color card = Color(0xFFFFFFFF);
  static const Color textMain = Color(0xFF1F2937);
  static const Color textMuted = Color(0xFF667085);
  static const Color warning = Color(0xFFB7791F);
  static const Color error = Color(0xFFB54747);

  static ThemeData get lightTheme {
    return ThemeData(
      primaryColor: primary,
      scaffoldBackgroundColor: backgroundMain,
      colorScheme: const ColorScheme.light(
        primary: primary,
        secondary: primaryDark,
        surface: card,
        error: error,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: card,
        foregroundColor: textMain,
        elevation: 1,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        ),
      ),
      cardTheme: CardTheme(
        color: card,
        elevation: 1,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }
}
